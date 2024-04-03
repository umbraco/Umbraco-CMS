import type { UmbRollbackModalData, UmbRollbackModalValue } from './rollback-modal.token.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../shared/document-variant-language-picker.element.js';
import UmbRollbackRepository from './repository/rollback.repository.js';
import { UmbUserRepository } from '@umbraco-cms/backoffice/user';
import { UmbLocalizeDateElement } from 'src/packages/core/localization/localize-date.element.js';

@customElement('umb-rollback-modal')
export class UmbRollbackModalElement extends UmbModalBaseElement<UmbRollbackModalData, UmbRollbackModalValue> {
	@state()
	versions: { date: string; user: string; isCurrentlyPublishedVersion: boolean; id: string }[] = [];

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

	#rollbackRepository = new UmbRollbackRepository(this);

	#userRepository = new UmbUserRepository(this);

	constructor() {
		super();

		this.#requestVersions();
	}

	async #requestVersions() {
		const { data } = await this.#rollbackRepository.requestVersionsByDocumentId(
			'bf327b58-9036-498b-9904-9b01b697b830',
			'en-us',
		);

		const tempItems: { date: string; user: string; isCurrentlyPublishedVersion: boolean; id: string }[] = [];

		data?.items.forEach((item: any) => {
			tempItems.push({
				date: item.versionDate,
				user: item.user.id,
				isCurrentlyPublishedVersion: item.isCurrentPublishedVersion,
				id: item.id,
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

		console.log('data', data);

		this.currentVersion = {
			date: version.date || '',
			user: version.user,
			name: data.variants[0].name,
			id: data.id,
			properties: data.values.map((value: any) => {
				return {
					alias: value.alias,
					value: value.value,
				};
			}),
		};
	}

	#onRollback() {
		console.log('Rollback');
		return;
		this.modalContext?.submit();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	async #onVersionClicked(id: string) {
		this.#setCurrentVersion(id);
		// this.currentVersion = { id };
		// console.log('Version clicked', id);

		// const { data } = await this.#rollbackRepository.requestVersionById(id);
		// console.log('datasss', data);
	}

	#renderVersions() {
		return repeat(
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
								<umb-localize-date date="${item.date}"></umb-localize-date>
							</p>
							<p>${item.user}</p>
							<p>${item.isCurrentlyPublishedVersion ? 'Current published version' : ''}</p>
						</div>
						<uui-button look="secondary" @click=${this.#onRollback}>Prevent cleanup</uui-button>
					</div>
				`;
			},
		);
	}

	#renderCurrentVersion() {
		if (!this.currentVersion) return;

		return html`
			<div>
				<h3>${this.currentVersion.name}</h3>
				${repeat(
					this.currentVersion.properties,
					(item) => item.alias,
					(item) => {
						return html` <p>${item.alias}: ${JSON.stringify(item.value)}</p> `;
					},
				)}
			</div>
		`;
	}

	get currentVersionHeader() {
		return this.localize.date(this.currentVersion?.date || '') + ' - ' + this.currentVersion?.user;
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
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.rollback-item.active {
				border-color: var(--uui-color-selected);
			}
			.rollback-item {
				border: 2px solid transparent;
				display: flex;
				justify-content: space-between;
				align-items: center;
				padding: var(--uui-size-space-5);
			}
			.rollback-item p {
				margin: 0;
				opacity: 0.5;
			}
			.rollback-item-date {
				opacity: 1;
			}
			.rollback-item uui-button {
				white-space: nowrap;
			}
			#main {
				display: flex;
				gap: var(--uui-size-space-4);
				width: 100%;
			}

			#box-left {
				--uui-box-default-padding: 0;
				max-width: 500px;
				flex: 1;
			}

			#box-right {
				flex: 1;
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
