// NOTE: `TextDirection` is now bundled with Tiptap since v3.11.0. [LK]
// https://github.com/ueberdosis/tiptap/pull/7207

// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import { extensions as TiptapExtensions } from '@tiptap/core';

/** @deprecated No longer used internally. This will be removed in Umbraco 19. [LK] */
export interface UmbTiptapTextDirectionOptions {
	directions: Array<'auto' | 'ltr' | 'rtl'>;
	types: Array<string>;
}

/** @deprecated No longer required, (since it comes default with Tiptap). This will be removed in Umbraco 19. [LK] */
export const TextDirection = TiptapExtensions.TextDirection.extend<UmbTiptapTextDirectionOptions>();
