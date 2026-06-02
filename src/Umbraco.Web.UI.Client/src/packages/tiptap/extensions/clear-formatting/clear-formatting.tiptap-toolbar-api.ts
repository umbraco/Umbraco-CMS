import type { CommandProps, Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarClearFormattingExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		const unsetAttrs: (props: CommandProps) => boolean = ({ commands }) => {
			commands.unsetClassName?.();
			commands.unsetStyles?.();
			return true;
		};
		editor?.chain().focus()?.clearNodes().unsetAllMarks().command(unsetAttrs).run();
	}
}
