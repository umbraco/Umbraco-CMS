import type { UmbInputDocumentPickerRootElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export type UmbTreePickerSource = {
	type?: UmbTreePickerSourceType;
	id?: string | null;
	dynamicRoot?: UmbTreePickerDynamicRoot | null;
};

export type UmbTreePickerSourceType = 'content' | 'member' | 'media';

export type UmbTreePickerDynamicRoot = {
	originAlias: string;
	querySteps?: Array<UmbTreePickerDynamicRootQueryStep> | null;
};

export type UmbTreePickerDynamicRootQueryStep = {
	alias: string;
	anyOfDocTypeKeys: Array<string>;
};

@customElement('umb-input-tree-picker-source')
export class UmbInputTreePickerSourceElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _type: UmbTreePickerSource['type'] = 'content';

	@property()
	public set type(value: UmbTreePickerSource['type']) {
		if (value === undefined) {
			value = this._type;
		}

		const oldValue = this._type;

		this._options = this._options.map((option) =>
			option.value === value ? { ...option, selected: true } : { ...option, selected: false },
		);

		this._type = value;

		this.requestUpdate('type', oldValue);
	}
	public get type(): UmbTreePickerSource['type'] {
		return this._type;
	}

	@property({ attribute: 'node-id' })
	nodeId?: string | null;

	@property({ attribute: false })
	dynamicRoot?: UmbTreePickerDynamicRoot | null;

	@state()
	_options: Array<Option> = [
		{ value: 'content', name: 'Content' },
		{ value: 'media', name: 'Media' },
		{ value: 'member', name: 'Members' },
	];

	#onTypeChange(event: UUISelectEvent) {
		event.stopPropagation();

		this.type = event.target.value as UmbTreePickerSource['type'];

		this.nodeId = '';

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onIdChange(event: CustomEvent) {
		switch (this.type) {
			case 'content':
				this.nodeId = (<UmbInputDocumentPickerRootElement>event.target).nodeId;
				break;
			case 'media':
			case 'member':
			default:
				break;
		}

		this.dispatchEvent(new CustomEvent(event.type));
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
			case 'member':
			default:
				return nothing;
		}
	}

	#renderTypeContent() {
		return html`<umb-input-document-picker-root
			@change=${this.#onIdChange}
			.nodeId=${this.nodeId}></umb-input-document-picker-root>`;
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

export default UmbInputTreePickerSourceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-tree-picker-source': UmbInputTreePickerSourceElement;
	}
}
