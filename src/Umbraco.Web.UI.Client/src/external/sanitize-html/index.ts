/* eslint local-rules/enforce-umbraco-external-imports: 0 */
import DOMPurify from 'dompurify';

const sanitizeHtml = DOMPurify.sanitize;

export { sanitizeHtml };
