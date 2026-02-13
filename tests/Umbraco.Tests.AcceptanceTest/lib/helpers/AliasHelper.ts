import camelize from 'camelize';

export class AliasHelper {
  /**
   * Uses Camelize npm library to generate a safe alias from a string
   * that may contain spaces and dashes etc
   *
   * @param  {string} text Input string
   * @returns {string} A camelcased string that starts with 'a' and ends with 'a'
   * @see {@link https://www.npmjs.com/package/camelize}
   */
  static toSafeAlias(text: string): string {
    return 'a' + camelize(text) + 'a';
  }

  /**
   * Camel cases a string by calling the toCamelCase() method
   *
   * @param  {string} text Input string
   * @returns {string} A camelcased string
   */
  static toAlias(text: string): string {
    return this.toCamelCase(text);
  }

  /**
   * Capatilze a string
   *
   * @param  {string} text Input string
   * @returns {string} A capatilized string, of the first character only
   */
  static capitalize(text: string): string {
    if (typeof text !== 'string') return '';
    return text.charAt(0).toUpperCase() + text.slice(1);
  }

  /**
   * Convert a sentence into camelCase
   * `toCamelCase('My aWesome Example')` would return `myAwesomeExample`
   *
   * @param  {string} sentenceCase Input string
   * @returns {string} A camel cased string
   */
  static toCamelCase(sentenceCase: string): string {
    let out = '';
    sentenceCase.split(' ').forEach((el, idx) => {
      const add = el.toLowerCase();
      out += idx === 0 ? add : add[0].toUpperCase() + add.slice(1);
    });
    return out;
  }

  /**
   * Removes dashes from UUID string
   *
   * @param  {string} uuid A string representing a UUID
   * @returns {string} UUID without dashes
   */
  static uuidToAlias(uuid: string): string {
    uuid = uuid.replace(/-/g, '');
    return this.toAlias(uuid);
  }
}
