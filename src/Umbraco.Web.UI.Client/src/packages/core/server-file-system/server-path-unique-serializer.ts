export class UmbServerFilePathUniqueSerializer {
	#magicDot = '%dot%';

	/**
	 * Converts a server file path to a unique URL friendly string that can be used in the client
	 * @param {string} serverFilePath
	 * @returns {*}  {(string | null)}
	 * @memberof UmbServerFilePathSerializer
	 */
	toUnique(serverFilePath: string): string {
		const urlSafeServerFilePath = serverFilePath?.replace('.', this.#magicDot);
		return encodeURIComponent(urlSafeServerFilePath);
	}

	/**
	 * Converts a unique URL friendly string to a server path
	 * @param {string} serverFilePathUnique
	 * @returns {*}  {(string | null)}
	 * @memberof UmbServerFilePathSerializer
	 */
	toServerPath(serverFilePathUnique: string | null): string | null {
		if (serverFilePathUnique === undefined) throw new Error('Server file path unique is missing');
		if (serverFilePathUnique === null) return null;
		const decoded = decodeURIComponent(serverFilePathUnique);
		return decoded.replace(this.#magicDot, '.');
	}
}
