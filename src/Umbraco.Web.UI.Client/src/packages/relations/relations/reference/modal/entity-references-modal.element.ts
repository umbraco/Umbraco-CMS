import type { UmbEntityReferenceListElement } from '../../global-components/entity-reference-list.element.js';
import type { UmbEntityReferencesModalData, UmbEntityReferencesModalValue } from './types.js';
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
	private _referencedByTotal = 0;

	@state()
	private _descendantsTotal = 0;

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

		const source = this.data.source;
		const showReferencedBy = !source || source === 'referencedBy';
		const showDescendants = !source || source === 'descendantsWithReferences';

		const headline =
			this.data.headline ??
			(source === 'descendantsWithReferences'
				? this.localize.term('references_labelDescendantsWithReferences')
				: this.localize.term('references_labelUsedByItems'));

		return html`
			<uui-dialog-layout headline=${headline}>
				${when(
					showReferencedBy,
					() => html`
						<div ?hidden=${!this._referencedByTotal}>
							<p>
								<umb-localize key="references_labelDependsOnThis">The following items depend on this</umb-localize>
							</p>
							<umb-entity-reference-list
								readonly
								.unique=${this.data.unique}
								.referenceRepositoryAlias=${this.data.referenceRepositoryAlias}
								source="referencedBy"
								@change=${this.#onReferencedByChange}>
							</umb-entity-reference-list>
						</div>
					`,
				)}
				${when(
					showDescendants,
					() => html`
						<div ?hidden=${!this._descendantsTotal}>
							<p>
								<umb-localize key="references_labelDependentDescendants"
									>The following descending items have dependencies</umb-localize
								>
							</p>
							<umb-entity-reference-list
								readonly
								.unique=${this.data.unique}
								.referenceRepositoryAlias=${this.data.referenceRepositoryAlias}
								.itemRepositoryAlias=${this.data.itemRepositoryAlias}
								source="descendantsWithReferences"
								@change=${this.#onDescendantsChange}>
							</umb-entity-reference-list>
						</div>
					`,
				)}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} look="primary" @click=${this.#close}></uui-button>
				</div>
			</uui-dialog-layout>
		`;
	}

	static override readonly styles = [
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
