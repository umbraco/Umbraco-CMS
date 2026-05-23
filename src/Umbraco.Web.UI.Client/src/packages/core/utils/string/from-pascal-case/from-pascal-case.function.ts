/**
 * Converts a PascalCase string to a human-readable label.
 * @param {string} str - The PascalCased string to convert.
 * @returns {string} - The converted human-readable label.
 * @example
 * const label = fromPascalCase('ScheduledPublish');
 * // label: 'Scheduled Publish'
 */
export const fromPascalCase = (str: string) => str.replace(/([a-z])([A-Z])/g, '$1 $2');
