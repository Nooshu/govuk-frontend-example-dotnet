import assert from 'node:assert/strict';
import { chmod, mkdir, mkdtemp, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import test from 'node:test';

import { commandEnv, dotnetExecutable } from '../scripts/dotnet-executable.mjs';

async function executable(directory, name) {
  await mkdir(directory, { recursive: true });
  const file = path.join(directory, name);
  await writeFile(file, '');
  await chmod(file, 0o755);
  return file;
}

test('uses dotnet from PATH before the user install', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const home = path.join(root, 'home');
  const bin = path.join(root, 'bin');
  await executable(bin, 'dotnet');
  await executable(path.join(home, '.dotnet'), 'dotnet');

  const found = dotnetExecutable({
    env: { PATH: `${path.delimiter}${bin}` },
    home,
    platform: 'darwin',
    installRoots: [],
  });
  assert.equal(found, path.join(bin, 'dotnet'));
});

test('uses the user profile install when PATH has no SDK', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const home = path.join(root, 'home');
  const installed = await executable(path.join(home, '.dotnet'), 'dotnet');

  const found = dotnetExecutable({
    env: { PATH: path.join(root, 'missing') },
    home,
    platform: 'linux',
    installRoots: [path.join(root, 'unused')],
  });
  assert.equal(found, installed);
});

test('uses a later install root when the user profile has no SDK', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const share = path.join(root, 'share');
  const installed = await executable(share, 'dotnet');

  const found = dotnetExecutable({
    env: {},
    home: path.join(root, 'empty-home'),
    platform: 'darwin',
    installRoots: [path.join(root, 'absent'), share],
  });
  assert.equal(found, installed);
});

test('reads the Windows Path variable and dotnet.exe', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const bin = path.join(root, 'bin');
  const installed = await executable(bin, 'dotnet.exe');

  const found = dotnetExecutable({
    env: { Path: bin },
    home: path.join(root, 'home'),
    platform: 'win32',
  });
  assert.equal(found, installed);
});

test('accepts a Windows dotnet file without the exe suffix', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const bin = path.join(root, 'bin');
  const installed = await executable(bin, 'dotnet');

  const found = dotnetExecutable({
    env: { PATH: bin },
    home: path.join(root, 'home'),
    platform: 'win32',
  });
  assert.equal(found, installed);
});

test('checks the standard Unix install locations by default', async () => {
  const root = await mkdtemp(path.join(tmpdir(), 'govuk-dotnet-'));
  const home = path.join(root, 'home');
  const installed = await executable(path.join(home, '.dotnet'), 'dotnet');

  const found = dotnetExecutable({
    env: { Path: path.join(root, 'unused') },
    home,
    platform: 'darwin',
  });
  assert.equal(found, installed);
});

test('returns null when no SDK is installed', () => {
  const found = dotnetExecutable({
    env: { PATH: '' },
    home: path.join(tmpdir(), 'govuk-dotnet-missing-home'),
    platform: 'win32',
  });
  assert.equal(found, null);
});

test('prepends the SDK directory when it is missing from PATH', () => {
  const env = commandEnv('/sdk/dotnet', { PATH: '/usr/bin' }, 'linux');
  assert.equal(env.PATH, `/sdk${path.delimiter}/usr/bin`);
});

test('leaves PATH unchanged when the SDK directory is already present', () => {
  const env = { PATH: `/sdk${path.delimiter}/usr/bin` };
  assert.equal(commandEnv('/sdk/dotnet', env, 'linux'), env);
});

test('sets PATH to the SDK directory when PATH is empty', () => {
  const env = commandEnv('/sdk/dotnet', {}, 'darwin');
  assert.equal(env.PATH, '/sdk');
});

test('sets Path on Windows', () => {
  const env = commandEnv('C:\\sdk\\dotnet.exe', { PATH: 'C:\\Windows' }, 'win32');
  assert.equal(env.Path, 'C:\\sdk;C:\\Windows');
});

test('keeps a Windows Path that already contains the SDK', () => {
  const env = { Path: 'C:\\sdk' };
  assert.equal(commandEnv('C:\\sdk\\dotnet.exe', env, 'win32'), env);
});
