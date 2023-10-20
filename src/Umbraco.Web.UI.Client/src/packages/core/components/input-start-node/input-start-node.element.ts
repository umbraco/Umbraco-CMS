import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, state, css, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIInputElement, UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { StartNode } from '@umbraco-cms/backoffice/components';

@customElement('umb-input-start-node')
export class UmbInputStartNodeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}
	private _startNodeQuery = '';

	@state()
	private queryTyping?: boolean;

	@property()
	startNodeType?: StartNode['type'];

	@property({ attribute: 'start-node-id' })
	startNodeId?: string;

	@property()
	public get startNodeQuery(): string {
		return this._startNodeQuery;
	}
	public set startNodeQuery(query: string) {
		this._startNodeQuery = query;
		query ? (this.queryTyping = true) : (this.queryTyping = false);
	}

	@property({ type: Array })
	options: Array<Option> = [];

	@query('#query')
	queryInput!: UUIInputElement;

	#onTypeChange(event: UUISelectEvent) {
		this.startNodeType = event.target.value as StartNode['type'];

		// Clear others
		this.startNodeQuery = '';
		this.startNodeId = '';
		this.dispatchEvent(new CustomEvent('change'));
	}

	#onIdChange(event: CustomEvent) {
		this.startNodeId = (event.target as UmbInputDocumentElement | UmbInputMediaElement).selectedIds.join('');
		this.dispatchEvent(new CustomEvent('change'));
	}

	#onQueryChange(event: UUIInputEvent) {
		this.startNodeQuery = event.target.value as string;
		this.dispatchEvent(new CustomEvent('change'));
	}

	#onQueryCancel() {
		this.queryTyping = false;
		this.queryInput.value = '';
		if (this.startNodeQuery) {
			this.startNodeQuery = '';
			this.dispatchEvent(new CustomEvent('change'));
		}
	}

	render() {
		return html`<umb-input-dropdown-list
				.options=${this.options}
				@change="${this.#onTypeChange}"></umb-input-dropdown-list>
			${this.renderType()}`;
	}

	renderType() {
		switch (this.startNodeType) {
			case 'content':
				return this.renderTypeContent();
			case 'media':
				return this.renderTypeMedia();
			case 'member':
				return this.renderTypeMember();
			default:
				return 'No type found';
		}
	}

	renderTypeContent() {
		const startNodeId = this.startNodeId ? [this.startNodeId] : [];

		if (this.startNodeQuery || this.queryTyping) {
			return html`<uui-input
					id="query"
					label="query"
					placeholder="Enter XPath query"
					@change=${this.#onQueryChange}
					value=${ifDefined(this.startNodeQuery)}>
				</uui-input>
				<uui-button label="Show XPath query help">
					<uui-icon name="umb:info" title="Show XPath query help"></uui-icon>Show XPath query help
				</uui-button>
				<uui-button label="Cancel and clear query" @click=${this.#onQueryCancel}>
					<uui-icon name="umb:backspace"></uui-icon>
					Clear & Cancel
				</uui-button>`;
		}

		return html`
			<umb-input-document
				@change=${this.#onIdChange}
				.selectedIds=${startNodeId}
				.query=${this.startNodeQuery}
				max="1"></umb-input-document>
			${!startNodeId.length ? this.renderQueryButton() : ''}
		`;
	}

	renderQueryButton() {
		return html`<uui-button
			label="Query for root node with XPath"
			@click=${() => (this.queryTyping = !this.queryTyping)}>
			<uui-icon name="umb:search"></uui-icon>Query for root node with XPath
		</uui-button>`;
	}

	renderTypeMedia() {
		const startNodeId = this.startNodeId ? [this.startNodeId] : [];
		//TODO => MediaTypes
		return html` <umb-input-media @change=${this.#onIdChange} .selectedIds=${startNodeId} max="1"></umb-input-media> `;
	}

	renderTypeMember() {
		const startNodeId = this.startNodeId ? [this.startNodeId] : [];
		//TODO => Members
		return html`
			<umb-input-member @change=${this.#onIdChange} .selectedIds=${startNodeId} max="1"></umb-input-member>
		`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-4);
			}
		`,
	];
}

export default UmbInputStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-start-node': UmbInputStartNodeElement;
	}
}
