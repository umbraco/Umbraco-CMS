import type { UmbCreatePartialViewFromSnippetModalData } from './index.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { PartialViewSnippetItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PartialViewService } from '@umbraco-cms/backoffice/external/backend-api';

interface UmbSnippetLinkModel {
	name: string;
	path: string;
}

@customElement('umb-partial-view-create-from-snippet-modal')
export class UmbPartialViewCreateFromSnippetModalElement extends UmbModalBaseElement<
	UmbCreatePartialViewFromSnippetModalData,
	string
> {
	@state()
	_snippets: Array<UmbSnippetLinkModel> = [];

	#getCreateHref(snippet: PartialViewSnippetItemResponseModel) {
		return `section/settings/workspace/partial-view/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}/snippet/${snippet.id}`;
	}

	protected override async firstUpdated() {
		const { data } = await tryExecuteAndNotify(this, PartialViewService.getPartialViewSnippet({ take: 10000 }));

		if (data) {
			this._snippets = data.items.map((snippet) => {
				return {
					name: snippet.name,
					path: this.#getCreateHref(snippet),
				};
			});
		}
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Partial View from snippet">
				<uui-box>
					${this._snippets.map(
						(snippet) =>
							html` <uui-menu-item label="${snippet.name}" href=${snippet.path} @click=${this.#onNavigate}>
								<uui-icon name="icon-document-html" slot="icon"></uui-icon
							></uui-menu-item>`,
					)}
				</uui-box>
				<uui-button slot="actions" @click=${this._rejectModal} look="secondary">Close</uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbPartialViewCreateFromSnippetModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-from-snippet-modal': UmbPartialViewCreateFromSnippetModalElement;
	}
}
