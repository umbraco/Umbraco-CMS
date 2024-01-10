import { UmbInputDocumentPickerRootElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, css, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
//import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export type ContentType = 'content' | 'member' | 'media';

export type DynamicRootQueryStepType = {
	alias: string;
	anyOfDocTypeKeys: Array<string>;
};

export type DynamicRootType = {
	originAlias: string;
	querySteps?: Array<DynamicRootQueryStepType> | null;
};

export type StartNode = {
	type?: ContentType;
	id?: string | null;
	dynamicRoot?: DynamicRootType | null;
};

@customElement('umb-input-tree-picker-source')
export class UmbInputTreePickerSourceElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _type: StartNode['type'] = 'content';

	@property()
	public set type(value: StartNode['type']) {
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
	public get type(): StartNode['type'] {
		return this._type;
	}

	@property({ attribute: 'node-id' })
	nodeId?: string | null;

	@property({ attribute: false })
	dynamicRoot?: DynamicRootType | null;

	@state()
	_options: Array<Option> = [
		{ value: 'content', name: 'Content' },
		{ value: 'media', name: 'Media' },
		{ value: 'member', name: 'Members' },
	];

	#onTypeChange(event: UUISelectEvent) {
		//console.log('onTypeChange');

		this.type = event.target.value as StartNode['type'];

		this.nodeId = '';

		// TODO: Appears that the event gets bubbled up. Will need to review. [LK]
		//this.dispatchEvent(new UmbChangeEvent());
	}

	#onIdChange(event: CustomEvent) {
		//console.log('onIdChange', event.target);
		switch (this.type) {
			case 'content':
				this.nodeId = (<UmbInputDocumentPickerRootElement>event.target).nodeId;
				break;
			case 'media':
				this.nodeId = (<UmbInputMediaElement>event.target).selectedIds.join('');
				break;
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
				return this.#renderTypeMedia();
			case 'member':
				return this.#renderTypeMember();
			default:
				return 'No type found';
		}
	}

	#renderTypeContent() {
		return html`<umb-input-document-picker-root
			@change=${this.#onIdChange}
			.nodeId=${this.nodeId}></umb-input-document-picker-root>`;
	}

	#renderTypeMedia() {
		const nodeId = this.nodeId ? [this.nodeId] : [];
		//TODO => MediaTypes
		return html`<umb-input-media @change=${this.#onIdChange} .selectedIds=${nodeId} max="1"></umb-input-media>`;
	}

	#renderTypeMember() {
		const nodeId = this.nodeId ? [this.nodeId] : [];
		//TODO => Members
		return html`<umb-input-member @change=${this.#onIdChange} .selectedIds=${nodeId} max="1"></umb-input-member>`;
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
