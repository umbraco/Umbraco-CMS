import { UmbPropertyTypeWorkspaceEditorElement } from './property-type-workspace-editor.element.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_CONTENT_TYPE_WORKSPACE_CONTEXT,
	UmbPropertyTypeModel,
	UmbPropertyTypeSettingsModalData,
} from '@umbraco-cms/backoffice/content-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbPropertyTypeData } from '../types.js';

export class UmbPropertyTypeWorkspaceContext<PropertyTypeData extends UmbPropertyTypeModel = UmbPropertyTypeModel>
	extends UmbSubmittableWorkspaceContextBase<PropertyTypeData>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_PROPERTY_TYPE_WORKSPACE_CONTEXT = true;

	#entityType: string;
	#data = new UmbObjectState<PropertyTypeData | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly unique = this.#data.asObservablePart((data) => data?.id);

	constructor(host: UmbControllerHost, args: { manifest: ManifestWorkspace }) {
		super(host, args.manifest.alias);
		const manifest = args.manifest;
		this.#entityType = manifest.meta?.entityType;

		this.routes.setRoutes([
			{
				// Would it make more sense to have groupKey before elementTypeKey?
				path: 'create/:containerUnique',
				component: UmbPropertyTypeWorkspaceEditorElement,
				setup: async (component, info) => {
					(component as UmbPropertyTypeWorkspaceEditorElement).workspaceAlias = manifest.alias;

					const containerUnique =
						info.match.params.containerUnique === 'null' ? null : info.match.params.containerUnique;
					this.create(containerUnique);

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
					(component as UmbPropertyTypeWorkspaceEditorElement).workspaceAlias = manifest.alias;

					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override resetState() {
		super.resetState();
		this.#data.setValue(undefined);
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		const context = await this.getContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT);
		this.observe(await context.structure.propertyStructureById(unique), (property) => {
			if (property) {
				this.#data.setValue(property as PropertyTypeData);
			}
			// Fallback to undefined:
			this.#data.setValue(undefined);
		});
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

		this.setIsNew(true);
		this.#data.setValue(data);
		return { data };
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
		this.#data.update({ name: name });
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof PropertyTypeData] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.getValue()?.[propertyAlias as keyof PropertyTypeData] as ReturnType;
	}

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
		const contentTypeUnique = (this.modalContext.data as unknown as UmbPropertyTypeSettingsModalData).contentTypeUnique;

		const data = this.#data.getValue();
		if (!data) {
			throw new Error('No data to submit.');
		}

		const context = await this.getContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT);

		context.structure.insertProperty(contentTypeUnique, data);

		this.setIsNew(false);
	}

	public override destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbPropertyTypeWorkspaceContext as api };
