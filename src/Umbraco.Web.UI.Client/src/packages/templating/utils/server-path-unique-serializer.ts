import { getParentPathFromServerPath } from './parent-path-from-server-path.function.js';

export class UmbServerPathUniqueSerializer {
	#magicDot = '%dot%';

	/**
	 * Converts a server path to a unique URL friendly string that can be used in the client
	 * @param {string} serverPath
	 * @return {*}  {(string | null)}
	 * @memberof UmbServerPathSerializer
	 */
	toUnique(serverPath: string): string | null {
		if (!serverPath) return null;
		const urlSafeServerPath = serverPath.replace('.', this.#magicDot);
		return encodeURIComponent(urlSafeServerPath);
	}

	/**
	 * Converts a server path to the a unique parent path URL friendly string that can be used in the client
	 * @param {string} serverPath
	 * @return {*}  {(string | null)}
	 * @memberof UmbServerPathUniqueSerializer
	 */
	toParentUnique(serverPath: string): string | null {
		const parentPath = getParentPathFromServerPath(serverPath);

		if (parentPath) {
			return this.toUnique(parentPath);
		} else {
			return null;
		}
	}

	/**
	 * Converts a unique URL friendly string to a server path
	 * @param {string} serverPathUnique
	 * @return {*}  {(string | null)}
	 * @memberof UmbServerPathSerializer
	 */
	toServerPath(serverPathUnique: string | null): string {
		if (serverPathUnique === undefined) throw new Error('Server path unique is missing');
		if (serverPathUnique === null) return '';
		const decoded = decodeURIComponent(serverPathUnique);
		return decoded.replace(this.#magicDot, '.');
	}
}
