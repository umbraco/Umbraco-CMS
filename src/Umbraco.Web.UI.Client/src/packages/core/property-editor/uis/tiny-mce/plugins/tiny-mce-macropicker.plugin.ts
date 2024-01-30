import type { TinyMcePluginArguments } from '@umbraco-cms/backoffice/components';
import { UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/components';

export default class UmbTinyMceMacroPickerPlugin extends UmbTinyMcePluginBase {
	constructor(args: TinyMcePluginArguments) {
		super(args);

		const contentStyle = this.editor.options.get('content_style');
		this.editor.options.set(
			'content_style',
			`
				${contentStyle ?? ''}
				.umb-macro-holder {
					border: 3px dotted var(--uui-palette-spanish-pink-light);
					padding: 7px;
					margin: 3px;
					display: block;
					position: relative;
				}
				.umb-macro-holder::after {
					content: 'Macros are no longer supported. Please use the block picker instead.';
					position: absolute;
					top: 50%;
					left: 50%;
					transform: translate(-50%, -50%);
					color: white;
					background-color: rgba(0, 0, 0, 0.7);
					padding: 10px;
					border-radius: 5px;
				}
			`,
		);

		/** when the contents load we need to find any macros declared and load in their content */
		this.editor.on('SetContent', () => {
			//get all macro divs and load their content
			this.editor.dom.select('.umb-macro-holder').forEach((macroElement: HTMLElement) => {
				macroElement.setAttribute('contenteditable', 'false');
			});
		});
	}
}
