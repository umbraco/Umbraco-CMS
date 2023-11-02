/* eslint local-rules/ensure-no-external-imports: 0 */
import DOMPurify from 'dompurify';

const sanitizeHtml = DOMPurify.sanitize;

export { sanitizeHtml };
