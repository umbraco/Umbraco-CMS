import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('story-modal-context-example')
export class StoryModalContextExampleElement extends UmbLitElement {
	@property()
	modalLayout = 'confirm';

	@state()
	value = '';

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _open() {
		// TODO: use the extension registry to get all modals
		/*
		switch (this.modalLayout) {
			case 'Content Picker':
				this._modalContext?.documentPicker();
				break;
			case 'Property Editor UI Picker':
				this._modalContext?.propertyEditorUIPicker();
				break;
			case 'Icon Picker':
				this._modalContext?.iconPicker();
				break;
			default:
				this._modalContext?.confirm({
					headline: 'Headline',
					content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit',
				});
				break;
		}
		*/
	}

	render() {
		return html`
			<uui-button label="open-dialog" look="primary" @click=${() => this._open()} style="margin-right: 9px;"
				>Open modal</uui-button
			>
		`;
	}
}
