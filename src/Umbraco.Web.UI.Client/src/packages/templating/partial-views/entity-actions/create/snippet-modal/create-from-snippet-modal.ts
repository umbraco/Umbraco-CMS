import { UmbCreatePartialViewFromSnippetModalData } from './create-from-snippet-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { PartialViewResource } from '@umbraco-cms/backoffice/backend-api';

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

	protected async firstUpdated() {
		const { data } = await tryExecuteAndNotify(this, PartialViewResource.getPartialViewSnippet({ take: 10000 }));

		if (data) {
			this._snippets = data.items.map((snippet) => {
				return {
					name: snippet.name,
					path: `section/settings/workspace/partial-view/create/${this.data?.parentUnique || 'null'}/${
						snippet.fileName
					}`,
				};
			});
		}
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Partial View from snippet">
				<div id="main">
					<uui-box>
						${this._snippets.map(
							(snippet) =>
								html`<uui-menu-item label="${snippet.name}" href=${snippet.path} @click=${
									this.#onNavigate
								}></uui-menu-item>
									<uui-icon name="icon-article" slot="icon"></uui-icon
								></uui-menu-item>`,
						)}
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._rejectModal} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPartialViewCreateFromSnippetModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-from-snippet-modal': UmbPartialViewCreateFromSnippetModalElement;
	}
}
