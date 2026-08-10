import type { UmbEntityReferencesModalData, UmbEntityReferencesModalValue } from './types.js';
import type { UmbEntityReferenceListElement } from '../../global-components/entity-reference-list.element.js';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import '../../global-components/entity-reference-list.element.js';

@customElement('umb-entity-references-modal')
export class UmbEntityReferencesModalElement extends UmbModalBaseElement<
	UmbEntityReferencesModalData,
	UmbEntityReferencesModalValue
> {
	@state()
	private _referencedByTotal?: number;

	@state()
	private _descendantsTotal?: number;

	#onReferencedByChange(event: UmbChangeEvent) {
		this._referencedByTotal = (event.target as UmbEntityReferenceListElement).getTotal();
	}

	#onDescendantsChange(event: UmbChangeEvent) {
		this._descendantsTotal = (event.target as UmbEntityReferenceListElement).getTotal();
	}

	#close() {
		this.modalContext?.submit();
	}

	override render() {
		if (!this.data) return nothing;

		const headline = this.data.headline ?? this.localize.term('references_labelUsedByItems');

		return html`
			<uui-dialog-layout headline=${headline}>
				${when(
					this._referencedByTotal !== 0,
					() => html`
						<p><umb-localize key="references_labelDependsOnThis"></umb-localize></p>
						<umb-entity-reference-list
							readonly
							.unique=${this.data!.unique}
							.referenceRepositoryAlias=${this.data!.referenceRepositoryAlias}
							source="referencedBy"
							@change=${this.#onReferencedByChange}></umb-entity-reference-list>
					`,
				)}
				${when(
					this._descendantsTotal !== 0,
					() => html`
						<p><umb-localize key="references_labelDependentDescendants"></umb-localize></p>
						<umb-entity-reference-list
							readonly
							.unique=${this.data!.unique}
							.referenceRepositoryAlias=${this.data!.referenceRepositoryAlias}
							.itemRepositoryAlias=${this.data!.itemRepositoryAlias}
							source="descendantsWithReferences"
							@change=${this.#onDescendantsChange}></umb-entity-reference-list>
					`,
				)}

				<div slot="actions">
					<uui-button
						label=${this.localize.term('general_close')}
						look="primary"
						@click=${this.#close}></uui-button>
				</div>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				min-width: 460px;
				max-width: 90vw;
			}

			h5 {
				margin-bottom: var(--uui-size-3);
			}
		`,
	];
}

export default UmbEntityReferencesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-references-modal': UmbEntityReferencesModalElement;
	}
}
