import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../core/context';
import { UmbModalService } from '../../core/services/modal';

@customElement('umb-property-editor-content-picker')
export class UmbPropertyEditorContentPicker extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}

			#add-button {
				width: 100%;
			}
		`,
	];

	private _modalService?: UmbModalService;

	@state()
	private _selectedContent: any[] = [];

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _open() {
		const modalHandler = this._modalService?.contentPicker({ multiple: true });
		modalHandler?.onClose.then(({ selection }: any) => {
			this._selectedContent = [...this._selectedContent, ...selection];
			this.requestUpdate('_selectedContent');
		});
	}

	private _removeContent(index: number, content: any) {
		const modalHandler = this._modalService?.confirm({
			color: 'danger',
			headline: 'Remove',
			confirmLabel: 'Remove',
			content: html`Remove <strong>${content.name}</strong>?`,
		});

		modalHandler?.onClose.then(({ confirmed }) => {
			if (confirmed) {
				this._selectedContent.splice(index, 1);
				this.requestUpdate('_selectedContent');
			}
		});
	}

	private _renderContent(content: any, index: number) {
		return html`
			<uui-ref-node name=${content.name} detail=${content.id}>
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeContent(index, content)}>Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	render() {
		return html`${this._selectedContent.map((content, index) => this._renderContent(content, index))}
			<uui-button id="add-button" look="placeholder" @click=${this._open} label="open">Add</uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-content-picker': UmbPropertyEditorContentPicker;
	}
}
