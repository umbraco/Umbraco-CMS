import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/index.js';
import type { UmbRollbackModalData, UmbRollbackModalValue } from './rollback-modal.token.js';
import { UmbRollbackRepository } from './repository/rollback.repository.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../shared/document-variant-language-picker.element.js';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

//TODO: Import through backoffice/internal
import { diffWords } from 'diff';

type DocumentVersion = {
	id: string;
	date: string;
	user: string;
	isCurrentlyPublishedVersion: boolean;
	preventCleanup: boolean;
};

@customElement('umb-rollback-modal')
export class UmbRollbackModalElement extends UmbModalBaseElement<UmbRollbackModalData, UmbRollbackModalValue> {
	@state()
	versions: DocumentVersion[] = [];

	@state()
	currentVersion?: {
		date: string;
		name: string;
		user: string;
		id: string;
		properties: {
			alias: string;
			value: string;
		}[];
	};

	@state()
	currentCulture?: string;

	@state()
	availableVariants: Option[] = [
		// { name: 'English', value: 'en-US', selected: true },
		// { name: 'Danish', value: 'da-dk' },
	];

	#rollbackRepository = new UmbRollbackRepository(this);
	#userItemRepository = new UmbUserItemRepository(this);

	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	#propertyDatasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;
	#variantWorkspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT.TYPE;

	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'long',
		hour: 'numeric',
		minute: '2-digit',
	};

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
			this.#propertyDatasetContext = instance;
			this.currentCulture = instance.getVariantId().culture ?? undefined;
			this.#requestVersions();
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
		});

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#variantWorkspaceContext = instance;

			// TODO: For some reason TS cant find languages on the context
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			instance.languages.subscribe((languages) => {
				this.availableVariants = languages.map((language: any) => {
					return {
						name: language.name,
						value: language.unique,
						selected: language.unique === this.currentCulture,
					};
				});
			});
		});
	}

	async #requestVersions() {
		if (!this.#propertyDatasetContext) return;

		const documentId = this.#propertyDatasetContext.getUnique();
		if (!documentId) return;

		const { data } = await this.#rollbackRepository.requestVersionsByDocumentId(documentId, this.currentCulture);
		if (!data) return;

		const tempItems: DocumentVersion[] = [];

		const uniqueUserIds = [...new Set(data?.items.map((item) => item.user.id))];

		const { data: userItems } = await this.#userItemRepository.requestItems(uniqueUserIds);

		data?.items.forEach((item: any) => {
			tempItems.push({
				date: item.versionDate,
				user: userItems?.find((user) => user.unique === item.user.id)?.name || 'Unknown user',
				isCurrentlyPublishedVersion: item.isCurrentPublishedVersion,
				id: item.id,
				preventCleanup: item.preventCleanup,
			});
		});

		this.versions = tempItems;
		const id = tempItems.find((item) => item.isCurrentlyPublishedVersion)?.id;

		if (id) {
			this.#setCurrentVersion(id);
		}
	}

	async #setCurrentVersion(id: string) {
		const version = this.versions.find((item) => item.id === id);
		if (!version) return;

		const { data } = await this.#rollbackRepository.requestVersionById(id);
		if (!data) return;

		this.currentVersion = {
			date: version.date,
			user: version.user,
			name: data.variants.find((x) => x.culture === this.currentCulture)?.name || data.variants[0].name,
			id: data.id,
			properties: data.values
				.filter((x) => x.culture === this.currentCulture)
				.map((value: any) => {
					return {
						alias: value.alias,
						value: value.value,
					};
				}),
		};
	}

	#onRollback() {
		if (!this.currentVersion) return;

		const id = this.currentVersion.id;
		const culture = this.availableVariants.length > 1 ? this.currentCulture : undefined;
		this.#rollbackRepository.rollback(id, culture);

		const docUnique = this.#workspaceContext?.getUnique() ?? '';
		this.#workspaceContext?.load(docUnique);
		this.modalContext?.reject();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	#onVersionClicked(id: string) {
		this.#setCurrentVersion(id);
	}

	#onPreventCleanup(event: Event, id: string, preventCleanup: boolean) {
		event.preventDefault();
		event.stopImmediatePropagation();
		this.#rollbackRepository.setPreventCleanup(id, preventCleanup);

		const version = this.versions.find((item) => item.id === id);
		if (!version) return;

		version.preventCleanup = preventCleanup;
		this.requestUpdate('versions');
	}

	#onChangeCulture(event: UUISelectEvent) {
		const value = event.target.value;

		this.currentCulture = value.toString();
		this.#requestVersions();
	}

	#renderCultureSelect() {
		return html`
			<div id="language-select">
				<b>Language</b>
				<uui-select @change=${this.#onChangeCulture} .options=${this.availableVariants}></uui-select>
			</div>
		`;
	}

	#renderVersions() {
		return html` ${this.#renderCultureSelect()}
		${repeat(
			this.versions,
			(item) => item.id,
			(item) => {
				return html`
					<div
						@click=${() => this.#onVersionClicked(item.id)}
						@keydown=${() => {}}
						class="rollback-item ${this.currentVersion?.id === item.id ? 'active' : ''}">
						<div>
							<p class="rollback-item-date">
								<umb-localize-date date="${item.date}" .options=${this.#localizeDateOptions}></umb-localize-date>
							</p>
							<p>${item.user}</p>
							<p>${item.isCurrentlyPublishedVersion ? 'Current published version' : ''}</p>
						</div>
						<uui-button
							look="secondary"
							@click=${(event: Event) => this.#onPreventCleanup(event, item.id, !item.preventCleanup)}>
							${item.preventCleanup ? 'Enable cleanup' : 'Prevent cleanup'}
						</uui-button>
					</div>
				`;
			},
		)}`;
	}

	#renderCurrentVersion() {
		if (!this.currentVersion) return;

		let draftValues =
			(this.#workspaceContext?.getData()?.values as Array<{ alias: string; culture: string; value: any }>) ?? [];

		draftValues = draftValues.filter((x) => x.culture === this.currentCulture);

		const diffs = this.currentVersion.properties.map((item) => {
			const draftValue = draftValues.find((x) => x.alias === item.alias);

			if (!draftValue) return;

			const draftValueString = trimQuotes(JSON.stringify(draftValue.value));
			const versionValueString = trimQuotes(JSON.stringify(item.value));

			const diff = diffWords(draftValueString, versionValueString);
			return { alias: item.alias, diff };
		});

		function trimQuotes(str: string): string {
			return str.replace(/^['"]|['"]$/g, '');
		}

		return html`
			<div id="diff">
				<p>name: ${this.currentVersion.name}</p>
				${repeat(
					this.currentVersion.properties,
					(item) => item.alias,
					(item) => {
						const diff = diffs.find((x) => x.alias === item.alias);
						return html`
							<p>
								${item.alias}:
								${diff
									? diff.diff.map((part) =>
											part.added
												? html`<span class="added">${part.value}</span>`
												: part.removed
													? html`<span class="removed">${part.value}</span>`
													: part.value,
										)
									: JSON.stringify(item.value)}
							</p>
						`;
					},
				)}
			</div>
		`;
	}

	get currentVersionHeader() {
		return (
			this.localize.date(this.currentVersion?.date || '', this.#localizeDateOptions) + ' - ' + this.currentVersion?.user
		);
	}

	render() {
		return html`
			<umb-body-layout headline="Rollback">
				<div id="main">
					<uui-box headline="Versions" id="box-left">
						<div>${this.#renderVersions()}</div>
					</uui-box>
					<uui-box headline=${this.currentVersionHeader} id="box-right"> ${this.#renderCurrentVersion()} </uui-box>
				</div>
				<umb-footer-layout slot="footer">
					<uui-button slot="actions" look="secondary" @click=${this.#onCancel}>Cancel</uui-button>
					<uui-button slot="actions" look="primary" @click=${this.#onRollback}>Rollback</uui-button>
				</umb-footer-layout>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#language-select {
				display: flex;
				flex-direction: column;
				padding: var(--uui-size-space-5);
				padding-bottom: 0;
				gap: var(--uui-size-space-2);
				font-size: 15px;
			}
			#diff .added {
				background-color: #d1ffdb;
			}
			#diff .removed {
				background-color: #ffd1d1;
			}
			.rollback-item {
				position: relative;
				display: flex;
				justify-content: space-between;
				align-items: center;
				padding: var(--uui-size-space-5);
				cursor: pointer;
			}
			.rollback-item::after {
				content: '';
				position: absolute;
				inset: 2px;
				display: block;
				border: 2px solid transparent;
				pointer-events: none;
			}
			.rollback-item.active::after,
			.rollback-item:hover::after {
				border-color: var(--uui-color-selected);
			}
			.rollback-item:not(.active):hover::after {
				opacity: 0.5;
			}
			.rollback-item p {
				margin: 0;
				opacity: 0.5;
			}
			p.rollback-item-date {
				opacity: 1;
			}
			.rollback-item uui-button {
				white-space: nowrap;
			}
			#main {
				display: flex;
				gap: var(--uui-size-space-4);
				width: 100%;
				height: 100%;
			}

			#box-left {
				--uui-box-default-padding: 0;
				max-width: 500px;
				flex: 1;
				overflow: auto;
				height: 100%;
			}

			#box-right {
				flex: 1;
				overflow: auto;
				height: 100%;
			}
		`,
	];
}

export default UmbRollbackModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rollback-modal': UmbRollbackModalElement;
	}
}
