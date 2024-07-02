import {
	type UmbSubmittableWorkspaceContext,
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbDictionaryDetailModel } from '../types.js';
import { UmbDictionaryDetailRepository } from '../repository/index.js';
import { UmbDictionaryWorkspaceEditorElement } from './dictionary-workspace-editor.element.js';

export class UmbDictionaryWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbDictionaryDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	//
	public readonly detailRepository = new UmbDictionaryDetailRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#data = new UmbObjectState<UmbDictionaryDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly entityType = this.#data.asObservablePart((data) => data?.entityType);

	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly dictionary = this.#data.asObservablePart((data) => data);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Dictionary');

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbDictionaryWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbDictionaryWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return 'dictionary';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(isoCode: string, translation: string) {
		if (!this.#data.value) return;

		// TODO: This can use some of our own methods, to make it simpler. see appendToFrozenArray()
		// update if the code already exists
		const updatedValue =
			this.#data.value.translations?.map((translationItem) => {
				if (translationItem.isoCode === isoCode) {
					return { ...translationItem, translation };
				}
				return translationItem;
			}) ?? [];

		// if code doesn't exist, add it to the new value set
		if (!updatedValue?.find((x) => x.isoCode === isoCode)) {
			updatedValue?.push({ isoCode, translation });
		}

		this.#data.setValue({ ...this.#data.value, translations: updatedValue });
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.detailRepository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);
	}

	async submit() {
		if (!this.#data.value) {
			throw new Error('No data to submit.');
		}
		if (!this.#data.value.unique) {
			throw new Error('No unique value to submit.');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) {
				throw new Error('Parent is not set');
			}
			const { error } = await this.detailRepository.create(this.#data.value, parent.unique);
			if (error) {
				throw new Error(error.message);
			}

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);

			this.setIsNew(false);
		} else {
			await this.detailRepository.save(this.#data.value);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	public override destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbDictionaryWorkspaceContext as api };
