import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-story-modal-context-example')
export class UmbStoryModalContextExampleElement extends UmbLitElement {
	@property()
	modalLayout = 'confirm';

	@state()
	value = '';

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
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
