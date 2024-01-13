export class UmbServerPathUniqueSerializer {
	#magicDot = '%dot%';

	/**
	 * Converts a server path to a unique URL friendly string that can be used in the client
	 * @param {string} serverPath
	 * @return {*}  {(string | null)}
	 * @memberof UmbServerPathSerializer
	 */
	toUnique(serverPath: string): string {
		const urlSafeServerPath = serverPath?.replace('.', this.#magicDot);
		return encodeURIComponent(urlSafeServerPath);
	}

	/**
	 * Converts a unique URL friendly string to a server path
	 * @param {string} serverPathUnique
	 * @return {*}  {(string | null)}
	 * @memberof UmbServerPathSerializer
	 */
	toServerPath(serverPathUnique: string | null): string | null {
		if (serverPathUnique === undefined) throw new Error('Server path unique is missing');
		if (serverPathUnique === null) return null;
		const decoded = decodeURIComponent(serverPathUnique);
		return decoded.replace(this.#magicDot, '.');
	}
}
