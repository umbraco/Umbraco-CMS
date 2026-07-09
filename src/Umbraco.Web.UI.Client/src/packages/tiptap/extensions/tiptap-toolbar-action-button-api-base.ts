import type { Editor } from '../externals.js';
import { UmbTiptapToolbarElementApiBase } from './tiptap-toolbar-element-api-base.js';

/**
 * Base class for `actionButton` kind toolbar extensions.
 *
 * Provides a default `isDisabled` implementation that returns `!isActive(editor)`,
 * meaning the button is disabled when the action cannot be performed.
 *
 * Extensions using this base class should override `isActive` to return whether the
 * action can be executed in the current editor state.
 */
export abstract class UmbTiptapToolbarActionButtonApiBase extends UmbTiptapToolbarElementApiBase {
	override isDisabled = (editor?: Editor) => !this.isActive(editor);
}
