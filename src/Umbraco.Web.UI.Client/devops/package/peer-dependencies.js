/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { readFileSync, existsSync } from 'fs';
import { join } from 'path';
import glob from 'tiny-glob';
import semver from 'semver';

/**
 * Converts a dependency range to the looser peer dependency range that allows plugin developers to use newer versions
 * while still enforcing a minimum version and safety ceiling.
 * @param {string} version - The dependency range, e.g. `^0.85.0` or `3.16.0`.
 * @returns {string} The peer dependency range, e.g. `>=0.85.0 <1.0.0` or `^3.16.0`.
 */
export const looseVersionRange = (version) => {
	// Extract minimum version from a range (e.g., ^0.85.0 -> 0.85.0)
	const minVersion = semver.minVersion(version);
	if (!minVersion) {
		console.warn('Could not parse version:', version, 'keeping original');
		return version;
	}

	const major = minVersion.major;
	const minor = minVersion.minor;
	const patch = minVersion.patch;

	// For pre-release (0.x.y), always use floor at current version and ceiling at 1.0.0
	if (major === 0) {
		return `>=${major}.${minor}.${patch} <1.0.0`;
	}

	// Exact version without caret, add caret (e.g., 3.16.0 -> ^3.16.0 and ^3.16.0 -> ^3.16.0 and ~3.16.0 -> ^3.16.0)
	return `^${major}.${minor}.${patch}`;
};

/**
 * Lists the dependencies that the published package declares as peer dependencies, in the order they are hoisted:
 * the dependencies of the root package.json first, then the dependencies of every workspace.
 * @param {object} packageJson - The root package.json.
 * @param {object} [options] - Options.
 * @param {string} [options.cwd] - The folder of the root package.json, which the workspace globs are relative to.
 * @param {string} [options.workspaceFolder] - The folder to read the workspace package.json files from, in place of `./src`.
 * @returns {Promise<Array<{ name: string, version: string, peerVersion: string, workspace?: string }>>} The dependencies.
 */
export const listPeerDependencies = async (packageJson, { cwd = './', workspaceFolder = './src' } = {}) => {
	const dependencies = Object.entries(packageJson.dependencies || {}).map(([name, version]) => ({ name, version }));

	const workspaces = packageJson.workspaces || [];
	const workspacePromises = workspaces.map(async (workspaceGlob) => {
		const localWorkspace = workspaceGlob.replace(/\.\/src/, workspaceFolder);
		const workspacePaths = await glob(localWorkspace, { cwd, absolute: true });

		workspacePaths.forEach((workspace) => {
			const workspacePackageFile = join(workspace, 'package.json');

			if (!existsSync(workspacePackageFile)) {
				console.warn(`No package.json found in workspace: ${workspace}`);
				return;
			}

			const workspacePackageJson = JSON.parse(readFileSync(workspacePackageFile, 'utf8'));
			Object.entries(workspacePackageJson.dependencies || {}).forEach(([name, version]) => {
				dependencies.push({ name, version, workspace });
			});
		});
	});

	await Promise.all(workspacePromises);

	return dependencies.map((dependency) => ({ ...dependency, peerVersion: looseVersionRange(dependency.version) }));
};

/**
 * Reduces listed dependencies to a `peerDependencies` object. A later entry overrides an earlier one of the same name.
 * @param {Array<{ name: string, peerVersion: string }>} dependencies - The dependencies from `listPeerDependencies`.
 * @returns {Record<string, string>} The peer dependencies.
 */
export const toPeerDependencies = (dependencies) =>
	Object.fromEntries(dependencies.map(({ name, peerVersion }) => [name, peerVersion]));
