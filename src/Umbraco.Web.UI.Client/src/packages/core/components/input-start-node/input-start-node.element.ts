import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, css, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export type ContentType = 'content' | 'member' | 'media';

export type StartNode = {
	type?: ContentType;
	id?: string | null;
	query?: string | null;
};

@customElement('umb-input-start-node')
export class UmbInputStartNodeElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _type: StartNode['type'] = 'content';
	@property()
	public set type(value: StartNode['type']) {
		const oldValue = this._type;

		this._options = this._options.map((option) =>
			option.value === value ? { ...option, selected: true } : { ...option, selected: false },
		);
		this._type = value;
		this.requestUpdate('type', oldValue);
	}
	public get type(): StartNode['type'] {
		return this._type;
	}

	@property({ attribute: 'node-id' })
	nodeId = '';

	@property({ attribute: 'dynamic-path' })
	dynamicPath = '';

	@state()
	_options: Array<Option> = [
		{ value: 'content', name: 'Content' },
		{ value: 'member', name: 'Members' },
		{ value: 'media', name: 'Media' },
	];

	#onTypeChange(event: UUISelectEvent) {
		this.type = event.target.value as StartNode['type'];

		// Clear others
		this.nodeId = '';
		this.dynamicPath = '';
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onIdChange(event: CustomEvent) {
		this.nodeId = (event.target as UmbInputDocumentElement | UmbInputMediaElement).selectedIds.join('');
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`<umb-input-dropdown-list
				.options=${this._options}
				@change="${this.#onTypeChange}"></umb-input-dropdown-list>
			${this.#renderType()}`;
	}

	#renderType() {
		switch (this.type) {
			case 'content':
				return this.#renderTypeContent();
			case 'media':
				return this.#renderTypeMedia();
			case 'member':
				return this.#renderTypeMember();
			default:
				return 'No type found';
		}
	}

	#renderTypeContent() {
		const nodeId = this.nodeId ? [this.nodeId] : [];
		//TODO: Dynamic paths
		return html` <umb-input-document @change=${this.#onIdChange} .selectedIds=${nodeId} max="1"></umb-input-document> `;
	}

	#renderTypeMedia() {
		const nodeId = this.nodeId ? [this.nodeId] : [];
		//TODO => MediaTypes
		return html` <umb-input-media @change=${this.#onIdChange} .selectedIds=${nodeId} max="1"></umb-input-media> `;
	}

	#renderTypeMember() {
		const nodeId = this.nodeId ? [this.nodeId] : [];
		//TODO => Members
		return html` <umb-input-member @change=${this.#onIdChange} .selectedIds=${nodeId} max="1"></umb-input-member> `;
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
