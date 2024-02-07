import type { UmbInputDocumentRootPickerElement } from '@umbraco-cms/backoffice/document';
import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export type UmbTreePickerSource = {
	type: UmbTreePickerSourceType;
	id?: string;
	dynamicRoot?: UmbTreePickerDynamicRoot;
};

export type UmbTreePickerSourceType = 'content' | 'member' | 'media';

export type UmbTreePickerDynamicRoot = {
	originAlias: string;
	originKey?: string;
	querySteps?: Array<UmbTreePickerDynamicRootQueryStep>;
};

export type UmbTreePickerDynamicRootQueryStep = {
	unique: string;
	alias: string;
	anyOfDocTypeKeys?: Array<string>;
};

@customElement('umb-input-tree-picker-source')
export class UmbInputTreePickerSourceElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	#type: UmbTreePickerSourceType = 'content';

	@property()
	public set type(value: UmbTreePickerSourceType) {
		if (value === undefined) {
			value = this.#type;
		}

		const oldValue = this.#type;

		this._options = this._options.map((option) =>
			option.value === value ? { ...option, selected: true } : { ...option, selected: false },
		);

		this.#type = value;

		this.requestUpdate('type', oldValue);
	}
	public get type(): UmbTreePickerSourceType {
		return this.#type;
	}

	@property({ attribute: 'node-id' })
	nodeId?: string;

	@property({ attribute: false })
	dynamicRoot?: UmbTreePickerDynamicRoot | undefined;

	@state()
	_options: Array<Option> = [
		{ value: 'content', name: 'Content' },
		{ value: 'media', name: 'Media' },
		{ value: 'member', name: 'Members' },
	];

	connectedCallback(): void {
		super.connectedCallback();

		// HACK: Workaround consolidating the old content-picker and dynamic-root. [LK:2024-01-24]
		if (this.nodeId && !this.dynamicRoot) {
			this.dynamicRoot = { originAlias: 'ByKey', originKey: this.nodeId, querySteps: [] };
		}
	}

	#onContentTypeChange(event: UUISelectEvent) {
		event.stopPropagation();

		this.type = event.target.value as UmbTreePickerSourceType;

		this.nodeId = undefined;
		this.dynamicRoot = undefined;

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onDocumentRootChange(event: CustomEvent) {
		switch (this.type) {
			case 'content':
				this.dynamicRoot = (event.target as UmbInputDocumentRootPickerElement).data;

				// HACK: Workaround consolidating the old content-picker and dynamic-root. [LK:2024-01-24]
				if (this.dynamicRoot?.originAlias === 'ByKey') {
					if (!this.dynamicRoot.querySteps || this.dynamicRoot.querySteps?.length === 0) {
						this.nodeId = this.dynamicRoot.originKey;
					} else {
						this.nodeId = undefined;
					}
				} else if (this.nodeId) {
					this.nodeId = undefined;
				}

				break;
			case 'media':
			case 'member':
			default:
				break;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`<umb-input-dropdown-list
				@change="${this.#onContentTypeChange}"
				.options=${this._options}></umb-input-dropdown-list>
			${this.#renderSourcePicker()}`;
	}

	#renderSourcePicker() {
		switch (this.type) {
			case 'content':
				return this.#renderDocumentSourcePicker();
			case 'media':
			case 'member':
			default:
				return nothing;
		}
	}

	#renderDocumentSourcePicker() {
		return html`<umb-input-document-root-picker
			@change=${this.#onDocumentRootChange}
			.data=${this.dynamicRoot}></umb-input-document-root-picker>`;
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
