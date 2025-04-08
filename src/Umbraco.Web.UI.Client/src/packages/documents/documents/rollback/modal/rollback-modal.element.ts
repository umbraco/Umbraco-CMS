import { UMB_DOCUMENT_ENTITY_TYPE } from '../../constants.js';
import { UmbRollbackRepository } from '../repository/rollback.repository.js';
import { UmbDocumentDetailRepository } from '../../repository/index.js';
import type { UmbDocumentDetailModel } from '../../types.js';
import type { UmbRollbackModalData, UmbRollbackModalValue } from './types.js';
import { diffWords, type UmbDiffChange } from '@umbraco-cms/backoffice/utils';
import { css, customElement, html, nothing, repeat, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageItemRepository } from '@umbraco-cms/backoffice/language';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

import '../../modals/shared/document-variant-language-picker.element.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';

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
	_versions: DocumentVersion[] = [];

	@state()
	_selectedVersion?: {
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
	_selectedCulture: string | null = null;

	@state()
	_isInvariant = true;

	@state()
	_availableVariants: Option[] = [];

	@state()
	_diffs: Array<{ alias: string; diff: UmbDiffChange[] }> = [];

	#rollbackRepository = new UmbRollbackRepository(this);
	#userItemRepository = new UmbUserItemRepository(this);

	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'long',
		hour: 'numeric',
		minute: '2-digit',
	};

	#currentDocument: UmbDocumentDetailModel | undefined;
	#currentAppCulture: string | undefined;
	#currentDatasetCulture: string | undefined;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (instance) => {
			this.#currentDatasetCulture = instance.getVariantId().culture ?? undefined;
			this.#selectCulture();
		});

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#currentAppCulture = instance.getAppCulture();
			this.#selectCulture();
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, async (instance) => {
			if (instance.getEntityType() !== UMB_DOCUMENT_ENTITY_TYPE) {
				throw new Error(`Entity type is not ${UMB_DOCUMENT_ENTITY_TYPE}`);
			}

			const unique = instance?.getUnique();

			if (!unique) {
				throw new Error('Document unique is not set');
			}

			const { data } = await new UmbDocumentDetailRepository(this).requestByUnique(unique);
			if (!data) return;

			this.#currentDocument = data;
			const itemVariants = this.#currentDocument?.variants ?? [];

			this._isInvariant = itemVariants.length === 1 && new UmbVariantId(itemVariants[0].culture).isInvariant();
			this.#selectCulture();

			const cultures = itemVariants.map((x) => x.culture).filter((x) => x !== null) as string[];
			const { data: languageItems } = await new UmbLanguageItemRepository(this).requestItems(cultures);

			if (languageItems) {
				this._availableVariants = languageItems.map((language) => {
					return {
						name: language.name,
						value: language.unique,
						selected: language.unique === this._selectedCulture,
					};
				});
			} else {
				this._availableVariants = [];
			}

			this.#requestVersions();
		});
	}

	#selectCulture() {
		const contextCulture = this.#currentDatasetCulture ?? this.#currentAppCulture ?? null;
		this._selectedCulture = this._isInvariant ? null : contextCulture;
	}

	async #requestVersions() {
		if (!this.#currentDocument?.unique) {
			throw new Error('Document unique is not set');
		}

		const { data } = await this.#rollbackRepository.requestVersionsByDocumentId(
			this.#currentDocument?.unique,
			this._selectedCulture ?? undefined,
		);
		if (!data) return;

		const tempItems: DocumentVersion[] = [];

		const uniqueUserIds = [...new Set(data?.items.map((item) => item.user.id))];

		const { data: userItems } = await this.#userItemRepository.requestItems(uniqueUserIds);

		data?.items.forEach((item: any) => {
			if (item.isCurrentDraftVersion) return;

			tempItems.push({
				date: item.versionDate,
				user:
					userItems?.find((user) => user.unique === item.user.id)?.name || this.localize.term('general_unknownUser'),
				isCurrentlyPublishedVersion: item.isCurrentPublishedVersion,
				id: item.id,
				preventCleanup: item.preventCleanup,
			});
		});

		this._versions = tempItems;
		const id = tempItems.find((item) => item.isCurrentlyPublishedVersion)?.id;

		if (id) {
			this.#selectVersion(id);
		}
	}

	async #selectVersion(id: string) {
		const version = this._versions.find((item) => item.id === id);

		if (!version) {
			this._selectedVersion = undefined;
			this._diffs = [];
			return;
		}

		const { data } = await this.#rollbackRepository.requestVersionById(id);

		if (!data) {
			this._selectedVersion = undefined;
			this._diffs = [];
			return;
		}

		this._selectedVersion = {
			date: version.date,
			user: version.user,
			name: data.variants.find((x) => x.culture === this._selectedCulture)?.name || data.variants[0].name,
			id: data.id,
			properties: data.values
				.filter((x) => x.culture === this._selectedCulture || !x.culture) // When invariant, culture is undefined or null.
				.map((value: any) => {
					return {
						alias: value.alias,
						value: value.value,
					};
				}),
		};

		await this.#setDiffs();
	}

	async #onRollback() {
		if (!this._selectedVersion) return;

		const id = this._selectedVersion.id;
		const culture = this._selectedCulture ?? undefined;

		const { error } = await this.#rollbackRepository.rollback(id, culture);
		if (error) return;

		const unique = this.#currentDocument?.unique;
		const entityType = this.#currentDocument?.entityType;

		if (!unique || !entityType) {
			throw new Error('Document unique or entity type is not set');
		}

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context not found');
		}

		const reloadStructureEvent = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });
		actionEventContext.dispatchEvent(reloadStructureEvent);

		const entityUpdatedEvent = new UmbEntityUpdatedEvent({ unique, entityType });
		actionEventContext.dispatchEvent(entityUpdatedEvent);

		this.modalContext?.submit();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	#onVersionClicked(id: string) {
		this.#selectVersion(id);
	}

	#onPreventCleanup(event: Event, id: string, preventCleanup: boolean) {
		event.preventDefault();
		event.stopImmediatePropagation();
		this.#rollbackRepository.setPreventCleanup(id, preventCleanup);

		const version = this._versions.find((item) => item.id === id);
		if (!version) return;

		version.preventCleanup = preventCleanup;
		this.requestUpdate('versions');
	}

	#onChangeCulture(event: UUISelectEvent) {
		const value = event.target.value;

		this._selectedCulture = value.toString();
		this.#requestVersions();
	}

	#trimQuotes(str: string): string {
		return str.replace(/^['"]|['"]$/g, '');
	}

	#renderCultureSelect() {
		return html`
			<uui-select
				id="language-select"
				@change=${this.#onChangeCulture}
				.options=${this._availableVariants}></uui-select>
		`;
	}

	#renderVersions() {
		if (!this._versions.length) {
			return html`<uui-box headline=${this.localize.term('rollback_versions')}>No versions available</uui-box>`;
		}

		return html` <uui-box id="versions-box" headline=${this.localize.term('rollback_versions')}>
			${repeat(
				this._versions,
				(item) => item.id,
				(item) => {
					return html`
						<div
							@click=${() => this.#onVersionClicked(item.id)}
							@keydown=${() => {}}
							class="rollback-item ${this._selectedVersion?.id === item.id ? 'active' : ''}">
							<div>
								<p class="rollback-item-date">
									<umb-localize-date date="${item.date}" .options=${this.#localizeDateOptions}></umb-localize-date>
								</p>
								<p>${item.user}</p>
								<p>${item.isCurrentlyPublishedVersion ? this.localize.term('rollback_currentPublishedVersion') : ''}</p>
							</div>
							<uui-button
								look="secondary"
								@click=${(event: Event) => this.#onPreventCleanup(event, item.id, !item.preventCleanup)}
								label=${item.preventCleanup
									? this.localize.term('contentTypeEditor_historyCleanupEnableCleanup')
									: this.localize.term('contentTypeEditor_historyCleanupPreventCleanup')}></uui-button>
						</div>
					`;
				},
			)}</uui-box
		>`;
	}

	async #setDiffs() {
		if (!this._selectedVersion) return;

		const currentPropertyValues = this.#currentDocument?.values.filter(
			(x) => x.culture === this._selectedCulture || !x.culture,
		); // When invariant, culture is undefined or null.

		if (!currentPropertyValues) {
			throw new Error('Current property values are not set');
		}

		const currentName = this.#currentDocument?.variants.find((x) => x.culture === this._selectedCulture)?.name;

		if (!currentName) {
			throw new Error('Current name is not set');
		}

		const diffs: Array<{ alias: string; diff: UmbDiffChange[] }> = [];

		const nameDiff = diffWords(currentName, this._selectedVersion.name);
		diffs.push({ alias: 'name', diff: nameDiff });

		this._selectedVersion.properties.forEach((item) => {
			const draftValue = currentPropertyValues.find((x) => x.alias === item.alias);

			if (!draftValue) return;

			const draftValueString = this.#trimQuotes(JSON.stringify(draftValue.value));
			const versionValueString = this.#trimQuotes(JSON.stringify(item.value));

			const diff = diffWords(draftValueString, versionValueString);
			diffs.push({ alias: item.alias, diff });
		});

		this._diffs = [...diffs];
	}

	#renderSelectedVersion() {
		if (!this._selectedVersion)
			return html`
				<uui-box id="box-right" style="display: flex; align-items: center; justify-content: center;"
					>No selected version</uui-box
				>
			`;

		return html`
			<uui-box headline=${this.currentVersionHeader} id="box-right">
				${unsafeHTML(this.localize.term('rollback_diffHelp'))}
				<uui-table>
					<uui-table-column style="width: 0"></uui-table-column>
					<uui-table-column></uui-table-column>

					<uui-table-head>
						<uui-table-head-cell>${this.localize.term('general_alias')}</uui-table-head-cell>
						<uui-table-head-cell>${this.localize.term('general_value')}</uui-table-head-cell>
					</uui-table-head>
					${repeat(
						this._diffs,
						(item) => item.alias,
						(item) => {
							const diff = this._diffs.find((x) => x?.alias === item.alias);
							return html`
								<uui-table-row>
									<uui-table-cell>${item.alias}</uui-table-cell>
									<uui-table-cell>
										${diff
											? diff.diff.map((part) =>
													part.added
														? html`<span class="diff-added">${part.value}</span>`
														: part.removed
															? html`<span class="diff-removed">${part.value}</span>`
															: part.value,
												)
											: nothing}
									</uui-table-cell>
								</uui-table-row>
							`;
						},
					)}
				</uui-table>
			</uui-box>
		`;
	}

	get currentVersionHeader() {
		return (
			this.localize.date(this._selectedVersion?.date ?? new Date(), this.#localizeDateOptions) +
			' - ' +
			this._selectedVersion?.user
		);
	}

	override render() {
		return html`
			<umb-body-layout headline="Rollback">
				<div id="main">
					<div id="box-left">
						${this._availableVariants.length
							? html`
									<uui-box id="language-box" headline=${this.localize.term('general_language')}>
										${this.#renderCultureSelect()}
									</uui-box>
								`
							: nothing}
						${this.#renderVersions()}
					</div>
					${this.#renderSelectedVersion()}
				</div>
				<umb-footer-layout slot="footer">
					<uui-button
						slot="actions"
						look="secondary"
						@click=${this.#onCancel}
						label=${this.localize.term('general_cancel')}></uui-button>
					<uui-button
						slot="actions"
						look="primary"
						@click=${this.#onRollback}
						label=${this.localize.term('actions_rollback')}
						?disabled=${!this._selectedVersion}></uui-button>
				</umb-footer-layout>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				color: var(--uui-color-text);
			}

			#language-box {
				margin-bottom: var(--uui-size-space-2);
			}

			#language-select {
				width: 100%;
			}

			uui-table {
				--uui-table-cell-padding: var(--uui-size-space-1) var(--uui-size-space-4);
				margin-top: var(--uui-size-space-5);
			}
			uui-table-head-cell:first-child {
				border-top-left-radius: var(--uui-border-radius);
			}
			uui-table-head-cell:last-child {
				border-top-right-radius: var(--uui-border-radius);
			}
			uui-table-head-cell {
				background-color: var(--uui-color-surface-alt);
			}
			uui-table-head-cell:last-child,
			uui-table-cell:last-child {
				border-right: 1px solid var(--uui-color-border);
			}
			uui-table-head-cell,
			uui-table-cell {
				border-top: 1px solid var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
			}
			uui-table-row:last-child uui-table-cell {
				border-bottom: 1px solid var(--uui-color-border);
			}
			uui-table-row:last-child uui-table-cell:last-child {
				border-bottom-right-radius: var(--uui-border-radius);
			}
			uui-table-row:last-child uui-table-cell:first-child {
				border-bottom-left-radius: var(--uui-border-radius);
			}

			.diff-added,
			ins {
				background-color: #00c43e63;
			}
			.diff-removed,
			del {
				background-color: #ff35356a;
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
				gap: var(--uui-size-space-5);
				width: 100%;
				height: 100%;
			}

			#versions-box {
				--uui-box-default-padding: 0;
			}

			#box-left {
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
