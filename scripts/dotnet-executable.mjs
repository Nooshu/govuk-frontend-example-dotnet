import { accessSync, constants } from 'node:fs';
import { homedir } from 'node:os';
import path from 'node:path';

const unixInstallRoots = ['/usr/local/share/dotnet', '/usr/share/dotnet'];

export function dotnetExecutable({
  env = process.env,
  home = homedir(),
  platform = process.platform,
  installRoots = platform === 'win32' ? [] : unixInstallRoots,
} = {}) {
  const fileName = platform === 'win32' ? 'dotnet.exe' : 'dotnet';
  const pathEntries = (env.PATH ?? env.Path ?? '').split(path.delimiter).filter(Boolean);

  for (const directory of pathEntries) {
    const onPath = path.join(directory, fileName);
    if (canRun(onPath)) {
      return onPath;
    }

    if (platform === 'win32') {
      const plain = path.join(directory, 'dotnet');
      if (canRun(plain)) {
        return plain;
      }
    }
  }

  const fallbacks = [
    path.join(home, '.dotnet', fileName),
    ...installRoots.map((root) => path.join(root, fileName)),
  ];
  for (const candidate of fallbacks) {
    if (canRun(candidate)) {
      return candidate;
    }
  }

  return null;
}

export function commandEnv(dotnet, env = process.env, platform = process.platform) {
  const paths = platform === 'win32' ? path.win32 : path;
  const directory = paths.dirname(dotnet);
  const pathKey = platform === 'win32' ? 'Path' : 'PATH';
  const current = env[pathKey] ?? env.PATH ?? '';
  const entries = current.split(paths.delimiter).filter(Boolean);
  if (entries.includes(directory)) {
    return env;
  }

  return {
    ...env,
    [pathKey]: current === '' ? directory : `${directory}${paths.delimiter}${current}`,
  };
}

function canRun(file) {
  try {
    accessSync(file, constants.X_OK);
    return true;
  } catch {
    return false;
  }
}
