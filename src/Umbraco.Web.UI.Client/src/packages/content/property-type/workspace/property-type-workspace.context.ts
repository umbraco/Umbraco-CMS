import { UmbDataPathPropertyTypeQuery } from '../utils/index.js';
import { UmbPropertyTypeWorkspaceEditorElement } from './property-type-workspace-editor.element.js';
import type { UmbPropertyTypeWorkspaceData } from './property-type-workspace.modal-token.js';
import type { UmbPropertyDatasetContext, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
	ManifestWorkspace,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
	umbObjectToPropertyValueArray,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';

export class UmbPropertyTypeWorkspaceContext<PropertyTypeData extends UmbPropertyTypeModel = UmbPropertyTypeModel>
	extends UmbSubmittableWorkspaceContextBase<PropertyTypeData>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_PROPERTY_TYPE_WORKSPACE_CONTEXT = true;

	#init: Promise<unknown>;
	#contentTypeContext?: typeof UMB_CONTENT_TYPE_WORKSPACE_CONTEXT.TYPE;

	#entityType: string;

	validationgContext: UmbValidationContext;

	// #persistedData
	// #currentData
	#data = new UmbObjectState<PropertyTypeData | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly unique = this.#data.asObservablePart((data) => data?.id);

	readonly values = this.#data.asObservablePart((data) => {
		return umbObjectToPropertyValueArray(data);
	});
	async getValues(): Promise<Array<UmbPropertyValueData> | undefined> {
		return umbObjectToPropertyValueArray(await firstValueFrom(this.data));
	}

	constructor(host: UmbControllerHost, args: { manifest: ManifestWorkspace }) {
		super(host, args.manifest.alias);

		const manifest = args.manifest;
		this.#entityType = manifest.meta?.entityType;

		this.validationgContext = new UmbValidationContext(this);
		this.addValidationContext(this.validationgContext);

		this.observe(this.unique, (unique) => {
			if (unique) {
				this.validationgContext.setDataPath(UmbDataPathPropertyTypeQuery({ id: unique }));
			}
		});

		this.#init = this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (context) => {
			this.#contentTypeContext = context;
		})
			.skipHost()
			.asPromise({ preventTimeout: true });

		this.routes.setRoutes([
			{
				// Would it make more sense to have groupKey before elementTypeKey?
				path: 'create/:containerUnique',
				component: UmbPropertyTypeWorkspaceEditorElement,
				setup: async (component, info) => {
					const containerUnique =
						info.match.params.containerUnique === 'null' ? null : info.match.params.containerUnique;
					await this.create(containerUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbPropertyTypeWorkspaceEditorElement,
				setup: (component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override resetState() {
		super.resetState();
		this.#data.setValue(undefined);
		this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
		this.removeUmbControllerByAlias('observePropertyTypeData');
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		await this.#init;
		this.observe(
			await this.#contentTypeContext?.structure.propertyStructureById(unique),
			(property) => {
				if (property) {
					this.#data.setValue(property as PropertyTypeData);
					//this.#persistedData.setValue(property);
					//this.#currentData.setValue(property);

					this.setIsNew(false);
				} else {
					// Fallback to undefined:
					this.#data.setValue(undefined);
				}
			},
			'observePropertyTypeData',
		);
	}

	async create(containerId?: string | null) {
		this.resetState();

		let data: PropertyTypeData = {
			id: UmbId.new(),
			container: containerId ? { id: containerId } : null,
			alias: '',
			name: '',
			description: '',
			variesByCulture: false,
			variesBySegment: false,
			validation: {
				mandatory: false,
				mandatoryMessage: null,
				regEx: null,
				regExMessage: null,
			},
			appearance: {
				labelOnTop: false,
			},
			sortOrder: 0,
		} as PropertyTypeData;

		// If we have a modal context, we blend in the modal preset data: [NL]
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}

		this.#data.setValue(data);
		//this.#persistedData.setValue(property);
		//this.#currentData.setValue(property);
		this.setIsNew(true);
	}

	getData() {
		return this.#data.getValue();
	}
	updateData(partialData: Partial<PropertyTypeData>) {
		this.#data?.update(partialData);
	}

	getUnique() {
		return this.getData()!.id;
	}

	getEntityType() {
		return this.#entityType;
	}

	getName() {
		return this.#data.getValue()?.name;
	}
	setName(name: string | undefined) {
		this.updateData({ name: name } as any);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof PropertyTypeData] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.getValue()?.[propertyAlias as keyof PropertyTypeData] as ReturnType;
	}

	/**
	 * @function setPropertyValue
	 * @param {string} propertyAlias
	 * @param alias
	 * @param {PromiseLike<unknown>} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#data.value;
		if (currentData) {
			this.#data.update({ ...currentData, [alias]: value });
		}
	}

	async submit() {
		if (!this.modalContext) {
			throw new Error('Needs to be in a modal to submit.');
		}
		const contentTypeUnique = (this.modalContext.data as UmbPropertyTypeWorkspaceData).contentTypeUnique;

		const data = this.#data.getValue();
		if (!data) {
			throw new Error('No data to submit.');
		}

		await this.#init;
		if (this.#contentTypeContext) {
			this.validationgContext.report();
			await this.#contentTypeContext.structure.insertProperty(contentTypeUnique, data);

			this.setIsNew(false);
		} else {
			throw new Error('Failed to find content type context.');
		}
	}

	public override destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbPropertyTypeWorkspaceContext as api };
