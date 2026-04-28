import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import {
	RedirectManagementService,
	RedirectStatusModel,
	type RedirectUrlResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-document-redirect-management-workspace-info-app')
export class UmbDocumentRedirectManagementWorkspaceInfoAppElement extends UmbLitElement {
	@state()
	private _isNew = false;

	@state()
	private _unique?: string;

	@state()
	private _trackerEnabled = true;

	@state()
	private _loading = false;

	@state()
	private _redirects: Array<RedirectUrlResponseModel> = [];

	#documentWorkspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#eventContext = context;

			this.#eventContext?.removeEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadRequest as unknown as EventListener,
			);

			this.#eventContext?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadRequest as unknown as EventListener,
			);
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#documentWorkspaceContext = context;

			this.observe(
				context?.isNew,
				(isNew) => {
					this._isNew = isNew === true;
				},
				'observeIsNew',
			);

			this.observe(
				context?.unique,
				(unique) => {
					if (!unique || unique === this._unique) return;
					this._unique = unique;
					this.#requestRedirects();
				},
				'observeUnique',
			);
		});

		this.#requestTrackerStatus();
	}

	async #requestTrackerStatus() {
		const { data } = await tryExecute(this, RedirectManagementService.getRedirectManagementStatus());
		if (data?.status) {
			this._trackerEnabled = data.status === RedirectStatusModel.ENABLED;
		}
	}

	async #requestRedirects() {
		if (this._isNew) return;
		if (!this._unique) return;

		this._loading = true;

		const { data } = await tryExecute(
			this,
			RedirectManagementService.getRedirectManagementById({ path: { id: this._unique } }),
		);

		this._redirects = data?.items ?? [];
		this._loading = false;
	}

	#debounceRequestRedirects = debounce(() => this.#requestRedirects(), 50);

	#onReloadRequest = (event: UmbEntityActionEvent) => {
		if (event.getUnique() !== this.#documentWorkspaceContext?.getUnique()) return;
		if (event.getEntityType() !== this.#documentWorkspaceContext.getEntityType()) return;
		this.#debounceRequestRedirects();
	};

	#getTargetUrl(url: string | undefined) {
		if (!url || url.length === 0) return url;
		if (url.includes('.') && !url.includes('//')) return '//' + url;
		return url;
	}

	override render() {
		if (!this._trackerEnabled) return nothing;
		if (this._isNew) return nothing;
		if (!this._loading && this._redirects.length === 0) return nothing;

		return html`
			<umb-workspace-info-app-layout headline="#redirectUrls_redirectUrlManagement">
				${when(
					this._loading,
					() => this.#renderLoading(),
					() => this.#renderContent(),
				)}
			</umb-workspace-info-app-layout>
		`;
	}

	#renderLoading() {
		return html`<div id="loader-container"><uui-loader></uui-loader></div>`;
	}

	#renderContent() {
		return html`
			<p id="panel-information">
				<umb-localize key="redirectUrls_panelInformation">
					The following URLs redirect to this content item:
				</umb-localize>
			</p>
			${repeat(
				this._redirects,
				(redirect) => redirect.id,
				(redirect) => this.#renderRedirect(redirect),
			)}
		`;
	}

	#renderRedirect(redirect: RedirectUrlResponseModel) {
		return html`
			<a
				class="redirect-item"
				href=${ifDefined(this.#getTargetUrl(redirect.originalUrl))}
				target="_blank"
				rel="noopener">
				<span>
					${redirect.culture ? html`<span class="culture">${redirect.culture}</span>` : nothing}
					<span>${redirect.originalUrl}</span>
				</span>
				<uui-icon name="icon-out"></uui-icon>
			</a>
		`;
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();

		this.#eventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadRequest as unknown as EventListener,
		);
	}

	static override styles = [
		css`
			#loader-container {
				display: flex;
				justify-content: center;
				align-items: center;
				padding: var(--uui-size-space-2);
			}

			#panel-information {
				margin: 0;
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				color: var(--uui-color-divider-emphasis);
			}

			.redirect-item {
				display: flex;
				justify-content: space-between;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				cursor: pointer;
				color: inherit;
				text-decoration: none;

				&:hover {
					background: var(--uui-color-divider);
				}

				& > span {
					display: flex;
					align-items: center;
					gap: var(--uui-size-6);
				}

				.culture {
					color: var(--uui-color-divider-emphasis);
				}
			}
		`,
	];
}

export default UmbDocumentRedirectManagementWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-redirect-management-workspace-info-app': UmbDocumentRedirectManagementWorkspaceInfoAppElement;
	}
}
