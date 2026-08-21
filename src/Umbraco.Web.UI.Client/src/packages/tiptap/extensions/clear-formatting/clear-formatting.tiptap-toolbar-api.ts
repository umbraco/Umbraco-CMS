import type { CommandProps, Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarClearFormattingExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		if (!editor) return;

		const unsetAttrs: (props: CommandProps) => boolean = ({ commands }) => {
			commands.unsetClassName?.();
			commands.unsetStyles?.();
			return true;
		};

		const marksToClear = Object.keys(editor.schema.marks).filter((markName) => markName !== 'umbLink' && markName !== 'link');

		const chain = editor.chain().focus().clearNodes();

		for (const markName of marksToClear) {
			chain.unsetMark(markName);
		}

		chain.command(unsetAttrs).run();
	}
}
