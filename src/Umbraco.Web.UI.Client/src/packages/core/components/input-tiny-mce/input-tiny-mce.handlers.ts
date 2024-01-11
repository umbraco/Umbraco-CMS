import type { RawEditorOptions } from '@umbraco-cms/backoffice/external/tinymce';

export const pastePreProcessHandler: RawEditorOptions['paste_preprocess'] = (_editor, args) => {
	// Remove spans
	args.content = args.content.replace(/<\s*span[^>]*>(.*?)<\s*\/\s*span>/g, '$1');
	// Convert b to strong.
	args.content = args.content.replace(/<\s*b([^>]*)>(.*?)<\s*\/\s*b([^>]*)>/g, '<strong$1>$2</strong$3>');
	// convert i to em
	args.content = args.content.replace(/<\s*i([^>]*)>(.*?)<\s*\/\s*i([^>]*)>/g, '<em$1>$2</em$3>');
};
