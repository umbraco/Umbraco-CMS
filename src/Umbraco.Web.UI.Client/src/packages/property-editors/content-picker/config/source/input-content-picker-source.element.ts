import type { UmbContentPickerDynamicRoot, UmbContentPickerSourceType } from '../../types.js';
import type { UmbInputContentPickerDocumentRootElement } from '../../dynamic-root/components/input-content-picker-document-root.element.js';
import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import '../../dynamic-root/components/input-content-picker-document-root.element.js';

@customElement('umb-input-content-picker-source')
export class UmbInputContentPickerSourceElement extends UUIFormControlMixin(UmbLitElement, '') {
	protected override getFormElement() {
		return undefined;
	}

	#type: UmbContentPickerSourceType = 'content';

	@property()
	public set type(value: UmbContentPickerSourceType) {
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
	public get type(): UmbContentPickerSourceType {
		return this.#type;
	}

	@property({ attribute: 'node-id' })
	nodeId?: string;

	@property({ attribute: false })
	dynamicRoot?: UmbContentPickerDynamicRoot;

	@state()
	_options: Array<Option> = [
		{ value: 'content', name: 'Content' },
		{ value: 'media', name: 'Media' },
		{ value: 'member', name: 'Members' },
	];

	override connectedCallback(): void {
		super.connectedCallback();

		// HACK: Workaround consolidating the old content-picker and dynamic-root. [LK:2024-01-24]
		if (this.nodeId && !this.dynamicRoot) {
			this.dynamicRoot = { originAlias: 'ByKey', originKey: this.nodeId, querySteps: [] };
		}
	}

	#onContentTypeChange(event: UUISelectEvent) {
		event.stopPropagation();

		this.type = event.target.value as UmbContentPickerSourceType;

		this.nodeId = undefined;
		this.dynamicRoot = undefined;

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onDocumentRootChange(event: CustomEvent & { target: UmbInputContentPickerDocumentRootElement }) {
		switch (this.type) {
			case 'content':
				this.dynamicRoot = event.target.data;

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

	override render() {
		return html`<umb-input-dropdown-list
				@change=${this.#onContentTypeChange}
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
		return html`
			<umb-input-content-picker-document-root .data=${this.dynamicRoot} @change=${this.#onDocumentRootChange}>
			</umb-input-content-picker-document-root>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-4);
			}
		`,
	];
}

export default UmbInputContentPickerSourceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-content-picker-source': UmbInputContentPickerSourceElement;
	}
}
