import type { UmbDuplicateToModalData, UmbDuplicateToModalValue } from './duplicate-to-modal.token.js';
import { html, customElement, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-duplicate-to-modal';
@customElement(elementName)
export class UmbDuplicateToModalElement extends UmbModalBaseElement<UmbDuplicateToModalData, UmbDuplicateToModalValue> {
	@state()
	_destinationUnique?: string | null;

	#onTreeSelectionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const selection = target.getSelection();
		this._destinationUnique = selection[0];

		if (this._destinationUnique || this._destinationUnique === null) {
			this.updateValue({ destination: { unique: this._destinationUnique } });
		}
	}

	override render() {
		if (!this.data) return nothing;

		return html`
			<umb-body-layout headline="Duplicate">
				<uui-box>
					<umb-tree
						alias=${this.data.treeAlias}
						.props=${{
							foldersOnly: this.data?.foldersOnly,
							expandTreeRoot: true,
						}}
						@selection-change=${this.#onTreeSelectionChange}></umb-tree>
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
				@click=${this._submitModal}
				?disabled=${this._destinationUnique === undefined}></uui-button>
		`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDuplicateToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDuplicateToModalElement;
	}
}
