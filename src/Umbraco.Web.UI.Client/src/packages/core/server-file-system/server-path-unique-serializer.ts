export class UmbServerFilePathUniqueSerializer {
	#magicDot = '%dot%';

	/**
	 * Converts a server file path to a unique URL friendly string that can be used in the client
	 * @param {string} serverFilePath - The server file path to convert
	 * @returns {string} The unique URL friendly string
	 * @memberof UmbServerFilePathUniqueSerializer
	 */
	toUnique(serverFilePath: string): string {
		const urlSafeServerFilePath = serverFilePath?.replace('.', this.#magicDot);
		return encodeURIComponent(urlSafeServerFilePath);
	}

	/**
	 * Converts a unique URL friendly string to a server path
	 * @param {string} serverFilePathUnique - The unique to convert
	 * @returns {string | null} The server file path, or null if the input was null
	 * @memberof UmbServerFilePathUniqueSerializer
	 */
	toServerPath(serverFilePathUnique: string | null): string | null {
		if (serverFilePathUnique === undefined) throw new Error('Server file path unique is missing');
		if (serverFilePathUnique === null) return null;
		const decoded = decodeURIComponent(serverFilePathUnique);
		return decoded.replace(this.#magicDot, '.');
	}
}
