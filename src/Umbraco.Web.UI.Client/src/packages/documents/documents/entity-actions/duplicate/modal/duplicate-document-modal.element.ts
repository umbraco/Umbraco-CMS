import { UMB_DOCUMENT_TREE_ALIAS } from '../../../tree/manifests.js';
import type {
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue,
} from './duplicate-document-modal.token.js';
import { html, customElement, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-document-duplicate-to-modal';
@customElement(elementName)
export class UmbDocumentDuplicateToModalElement extends UmbModalBaseElement<
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue
> {
	#onTreeSelectionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const selection = target.getSelection();
		if (selection.length === 0) throw new Error('Selection is required');
		this.updateValue({ destination: { unique: selection[0] } });
	}

	#onRelateToOriginalChange(event: UUIBooleanInputEvent) {
		const target = event.target;
		this.updateValue({ relateToOriginal: target.checked });
	}

	#onIncludeDescendantsChange(event: UUIBooleanInputEvent) {
		const target = event.target;
		this.updateValue({ includeDescendants: target.checked });
	}

	render() {
		if (!this.data) return nothing;

		return html`
			<umb-body-layout headline="Duplicate">
				<uui-box id="tree-box" headline="Duplicate to">
					<umb-tree alias=${UMB_DOCUMENT_TREE_ALIAS} @selection-change=${this.#onTreeSelectionChange}></umb-tree>
				</uui-box>
				<uui-box headline="Options">
					<umb-property-layout label="Relate to original" orientation="vertical"
						><div slot="editor">
							<uui-toggle
								@change=${this.#onRelateToOriginalChange}
								.checked=${this.value?.relateToOriginal}></uui-toggle>
						</div>
					</umb-property-layout>

					<umb-property-layout label="Include descendants" orientation="vertical"
						><div slot="editor">
							<uui-toggle
								@change=${this.#onIncludeDescendantsChange}
								.checked=${this.value?.includeDescendants}></uui-toggle>
						</div>
					</umb-property-layout>
				</uui-box>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderActions() {
		return html`
			<uui-button slot="actions" label="Cancel" @click="${this._rejectModal}"></uui-button>
			<uui-button
				slot="actions"
				color="positive"
				look="primary"
				label="Duplicate"
				@click=${this._submitModal}></uui-button>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#tree-box {
				margin-bottom: var(--uui-size-layout-1);
			}
		`,
	];
}

export { UmbDocumentDuplicateToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentDuplicateToModalElement;
	}
}
