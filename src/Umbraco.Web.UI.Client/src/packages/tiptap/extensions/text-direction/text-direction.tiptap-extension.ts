// NOTE: `TextDirection` is now bundled with Tiptap since v3.11.0. [LK]
// https://github.com/ueberdosis/tiptap/pull/7207

import { extensions as TiptapExtensions } from '../../externals.js';

/** @deprecated No longer used internally. This will be removed in Umbraco 19. [LK] */
export interface UmbTiptapTextDirectionOptions {
	directions: Array<'auto' | 'ltr' | 'rtl'>;
	types: Array<string>;
}

/** @deprecated No longer required, (since it comes default with Tiptap). This will be removed in Umbraco 19. [LK] */
export const TextDirection = TiptapExtensions.TextDirection;
