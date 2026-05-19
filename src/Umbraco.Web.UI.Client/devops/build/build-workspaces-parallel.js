#!/usr/bin/env node
// Cross-platform parallel runner for `vite build` across every workspace
// under src/{libs,packages,external}. Drop-in replacement for
// `npm run build:workspaces` (which runs serially). On jov's M-series Mac the
// serial baseline is ~49s; this script finishes in ~28s using -P 8.

import { spawn } from 'child_process';
import { cpus } from 'os';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const clientRoot = path.resolve(__dirname, '../../');

const concurrency = Number(process.env.UMB_BUILD_CONCURRENCY) || Math.min(8, Math.max(2, cpus().length));

const groups = ['src/libs', 'src/packages', 'src/external'];
const workspaces = [];
for (const group of groups) {
	const full = path.join(clientRoot, group);
	if (!fs.existsSync(full)) continue;
	for (const name of fs.readdirSync(full).sort()) {
		const ws = path.join(full, name);
		if (fs.existsSync(path.join(ws, 'vite.config.ts'))) workspaces.push(ws);
	}
}

console.log(`Building ${workspaces.length} workspaces in parallel (concurrency=${concurrency})...`);
const started = Date.now();

let nextIndex = 0;
let active = 0;
let failures = 0;
/** @type {Set<import('child_process').ChildProcess>} */
const liveChildren = new Set();

// Ensure children don't outlive us if the parent gets interrupted.
function killAllChildren() {
	for (const child of liveChildren) {
		try {
			child.kill('SIGTERM');
		} catch {
			// already gone
		}
	}
}
process.on('SIGINT', () => {
	console.error('\nInterrupted — killing in-flight builds...');
	killAllChildren();
	process.exit(130);
});
process.on('SIGTERM', () => {
	killAllChildren();
	process.exit(143);
});

await new Promise((resolve) => {
	function tick() {
		while (active < concurrency && nextIndex < workspaces.length) {
			const ws = workspaces[nextIndex++];
			active++;
			const rel = path.relative(clientRoot, ws);
			const t0 = Date.now();
			// stdout MUST be ignored (not piped) — vite prints a chunk listing
			// per build and packages/core alone emits 981 lines. An undrained
			// pipe buffer fills up and the child blocks on stdout.write,
			// hanging the whole fan-out. stderr is captured so we can surface
			// failures with context.
			const child = spawn(process.platform === 'win32' ? 'npx.cmd' : 'npx', ['vite', 'build'], {
				cwd: ws,
				stdio: ['ignore', 'ignore', 'pipe'],
			});
			liveChildren.add(child);
			let stderr = '';
			child.stderr.on('data', (d) => (stderr += d.toString()));
			child.on('error', (err) => {
				// spawn() itself failed (e.g. npx missing). Treat as a failure
				// and let the exit handler tally it. spawn errors emit BEFORE
				// any exit, so guard against double-decrement.
				stderr += `\nspawn error: ${err.message}\n`;
			});
			child.on('exit', (code) => {
				liveChildren.delete(child);
				active--;
				const dt = ((Date.now() - t0) / 1000).toFixed(1);
				if (code === 0) {
					console.log(`  ✓ ${rel}  (${dt}s)`);
				} else {
					failures++;
					console.error(`  ✗ ${rel}  (${dt}s, exit ${code})`);
					if (stderr) console.error(stderr.split('\n').slice(-15).join('\n'));
				}
				tick();
				if (active === 0 && nextIndex >= workspaces.length) resolve();
			});
		}
	}
	tick();
});

const elapsed = ((Date.now() - started) / 1000).toFixed(1);
console.log(`Done in ${elapsed}s (${failures} failure${failures === 1 ? '' : 's'}).`);
process.exit(failures > 0 ? 1 : 0);
