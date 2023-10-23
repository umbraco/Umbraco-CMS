import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { StartNode } from '@umbraco-cms/backoffice/components';

@customElement('umb-input-start-node')
export class UmbInputStartNodeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@property()
	startNodeType?: StartNode['type'];

	@property({ attribute: 'start-node-id' })
	startNodeId?: string;

	@property({ type: Array })
	options: Array<Option> = [];

	#onTypeChange(event: UUISelectEvent) {
		this.startNodeType = event.target.value as StartNode['type'];

		// Clear others
		this.startNodeId = '';
		this.dispatchEvent(new CustomEvent('change'));
	}

	#onIdChange(event: CustomEvent) {
		this.startNodeId = (event.target as UmbInputDocumentElement | UmbInputMediaElement).selectedIds.join('');
		this.dispatchEvent(new CustomEvent('change'));
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
		return html`
			<umb-input-document @change=${this.#onIdChange} .selectedIds=${startNodeId} max="1"></umb-input-document>
		`;
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
