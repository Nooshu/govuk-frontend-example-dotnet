import { spawn } from 'node:child_process';

import { commandEnv, dotnetExecutable } from './dotnet-executable.mjs';

const dotnet = dotnetExecutable();
if (dotnet === null) {
  process.stderr.write('The .NET 10 SDK was not found. Install it, or add its folder to PATH.\n');
  process.exit(1);
}

const child = spawn(dotnet, ['run', '--project', 'src/GovUk.Frontend.Example'], {
  stdio: 'inherit',
  env: commandEnv(dotnet),
});

child.on('exit', (code, signal) => {
  if (signal !== null) {
    process.kill(process.pid, signal);
    return;
  }

  process.exitCode = code ?? 1;
});
