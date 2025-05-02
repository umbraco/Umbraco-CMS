/**
 * Splits an array into chunks of a specified size
 * @param { Array<BatchEntryType> } array - The array to split
 * @param {number }batchSize - The size of each chunk
 * @returns {Array<Array<T>>} - An array of chunks
 */
export function batchArray<BatchEntryType>(
	array: Array<BatchEntryType>,
	batchSize: number,
): Array<Array<BatchEntryType>> {
	const chunks: Array<Array<BatchEntryType>> = [];
	for (let i = 0; i < array.length; i += batchSize) {
		chunks.push(array.slice(i, i + batchSize));
	}
	return chunks;
}
