import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import { UMB_USER_ENTITY_TYPE } from '../../../../entity.js';
import { UmbUserWorkspaceAssignAccessElement } from './user-workspace-assign-access.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type { UmbStartNodeAccessValue } from '@umbraco-cms/backoffice/property-editor';

/** Stands in for `UmbUserWorkspaceContext`, exposing only the access-related state the element reads and writes. */
class UmbTestUserWorkspaceContext {
	#host: UmbControllerHostElement;

	#userGroupUniques = new UmbArrayState<UmbReferenceByUnique>([], (x) => x.unique);
	readonly userGroupUniques = this.#userGroupUniques.asObservable();

	#hasDocumentRootAccess = new UmbBooleanState(false);
	readonly hasDocumentRootAccess = this.#hasDocumentRootAccess.asObservable();
	#documentStartNodeUniques = new UmbArrayState<UmbReferenceByUnique>([], (x) => x.unique);
	readonly documentStartNodeUniques = this.#documentStartNodeUniques.asObservable();

	#hasMediaRootAccess = new UmbBooleanState(false);
	readonly hasMediaRootAccess = this.#hasMediaRootAccess.asObservable();
	#mediaStartNodeUniques = new UmbArrayState<UmbReferenceByUnique>([], (x) => x.unique);
	readonly mediaStartNodeUniques = this.#mediaStartNodeUniques.asObservable();

	#hasElementRootAccess = new UmbBooleanState(false);
	readonly hasElementRootAccess = this.#hasElementRootAccess.asObservable();
	#elementStartNodeUniques = new UmbArrayState<UmbReferenceByUnique>([], (x) => x.unique);
	readonly elementStartNodeUniques = this.#elementStartNodeUniques.asObservable();

	readonly setUserGroupsCalls: Array<Array<UmbReferenceByUnique>> = [];
	readonly setDocumentAccessCalls: Array<UmbStartNodeAccessValue> = [];
	readonly setMediaAccessCalls: Array<UmbStartNodeAccessValue> = [];
	readonly setElementAccessCalls: Array<UmbStartNodeAccessValue> = [];

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	// The context token only resolves providers whose entity type matches the user workspace.
	getEntityType() {
		return UMB_USER_ENTITY_TYPE;
	}

	setUserGroupUniques(uniques: Array<UmbReferenceByUnique>) {
		this.#userGroupUniques.setValue(uniques);
	}

	setDocumentAccessState(value: UmbStartNodeAccessValue) {
		this.#hasDocumentRootAccess.setValue(value.rootAccess);
		this.#documentStartNodeUniques.setValue(value.startNodes);
	}

	setMediaAccessState(value: UmbStartNodeAccessValue) {
		this.#hasMediaRootAccess.setValue(value.rootAccess);
		this.#mediaStartNodeUniques.setValue(value.startNodes);
	}

	setElementAccessState(value: UmbStartNodeAccessValue) {
		this.#hasElementRootAccess.setValue(value.rootAccess);
		this.#elementStartNodeUniques.setValue(value.startNodes);
	}

	setUserGroups(uniques: Array<UmbReferenceByUnique>) {
		this.setUserGroupsCalls.push(uniques);
	}

	setDocumentAccess(value: UmbStartNodeAccessValue) {
		this.setDocumentAccessCalls.push(value);
	}

	setMediaAccess(value: UmbStartNodeAccessValue) {
		this.setMediaAccessCalls.push(value);
	}

	setElementAccess(value: UmbStartNodeAccessValue) {
		this.setElementAccessCalls.push(value);
	}
}

@customElement('umb-test-assign-access-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestAssignAccessHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbUserWorkspaceAssignAccessElement', () => {
	let host: UmbTestAssignAccessHostElement;
	let element: UmbUserWorkspaceAssignAccessElement;
	let context: UmbTestUserWorkspaceContext;
	let dataset: UmbPropertyDatasetContext;

	beforeEach(async () => {
		host = await fixture(
			html`<umb-test-assign-access-host>
				<umb-user-workspace-assign-access></umb-user-workspace-assign-access>
			</umb-test-assign-access-host>`,
		);
		element = host.querySelector('umb-user-workspace-assign-access') as UmbUserWorkspaceAssignAccessElement;

		context = new UmbTestUserWorkspaceContext(host);
		host.provideContext(UMB_USER_WORKSPACE_CONTEXT, context as never);
		await aTimeout(0);

		// The rendered `umb-property` elements read from the dataset hosted by the element's own
		// `umb-property-dataset`, so it's the property editor UIs' view of the workspace data — not an
		// implementation detail.
		const datasetElement = element.shadowRoot!.querySelector('umb-property-dataset')!;
		dataset = (datasetElement as unknown as { context: UmbPropertyDatasetContext }).context;
	});

	async function datasetValueByAlias<ValueType>(alias: string) {
		const source = await dataset.propertyValueByAlias<ValueType>(alias);
		return firstValueFrom(source!);
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbUserWorkspaceAssignAccessElement);
	});

	describe('workspace context → property dataset', () => {
		it('mirrors the selected user groups', async () => {
			context.setUserGroupUniques([{ unique: 'group-1' }]);
			await aTimeout(0);

			expect(await datasetValueByAlias('userGroups')).to.deep.equal([{ unique: 'group-1' }]);
		});

		it('merges document root access and start nodes into a single value', async () => {
			context.setDocumentAccessState({ rootAccess: false, startNodes: [{ unique: 'doc-1' }] });
			await aTimeout(0);

			expect(await datasetValueByAlias('documentAccess')).to.deep.equal({
				rootAccess: false,
				startNodes: [{ unique: 'doc-1' }],
			});
		});

		it('merges media root access and start nodes into a single value', async () => {
			context.setMediaAccessState({ rootAccess: true, startNodes: [] });
			await aTimeout(0);

			expect(await datasetValueByAlias('mediaAccess')).to.deep.equal({ rootAccess: true, startNodes: [] });
		});

		it('merges element root access and start nodes into a single value', async () => {
			context.setElementAccessState({ rootAccess: true, startNodes: [] });
			await aTimeout(0);

			expect(await datasetValueByAlias('elementAccess')).to.deep.equal({ rootAccess: true, startNodes: [] });
		});

		it('does not write workspace-originated values back to the workspace context', async () => {
			// A value pushed in from the workspace (e.g. the workspace data being cleared after deletion) must not
			// echo back into a `set*` call, or an unrelated navigation will look like it has unpersisted changes.
			context.setUserGroupUniques([{ unique: 'group-1' }]);
			context.setDocumentAccessState({ rootAccess: false, startNodes: [{ unique: 'doc-1' }] });
			context.setMediaAccessState({ rootAccess: true, startNodes: [] });
			context.setElementAccessState({ rootAccess: true, startNodes: [] });
			await aTimeout(0);

			expect(context.setUserGroupsCalls).to.be.empty;
			expect(context.setDocumentAccessCalls).to.be.empty;
			expect(context.setMediaAccessCalls).to.be.empty;
			expect(context.setElementAccessCalls).to.be.empty;
		});
	});

	describe('property dataset → workspace context', () => {
		it('writes a changed user group selection back to the workspace context', async () => {
			dataset.setPropertyValue('userGroups', [{ unique: 'group-2' }]);
			await aTimeout(0);

			expect(context.setUserGroupsCalls.at(-1)).to.deep.equal([{ unique: 'group-2' }]);
		});

		it('writes a changed document access value back to the workspace context', async () => {
			const value: UmbStartNodeAccessValue = { rootAccess: true, startNodes: [] };
			dataset.setPropertyValue('documentAccess', value);
			await aTimeout(0);

			expect(context.setDocumentAccessCalls.at(-1)).to.deep.equal(value);
		});

		it('writes a changed media access value back to the workspace context', async () => {
			const value: UmbStartNodeAccessValue = { rootAccess: false, startNodes: [{ unique: 'media-1' }] };
			dataset.setPropertyValue('mediaAccess', value);
			await aTimeout(0);

			expect(context.setMediaAccessCalls.at(-1)).to.deep.equal(value);
		});

		it('writes a changed element access value back to the workspace context', async () => {
			const value: UmbStartNodeAccessValue = { rootAccess: false, startNodes: [{ unique: 'element-1' }] };
			dataset.setPropertyValue('elementAccess', value);
			await aTimeout(0);

			expect(context.setElementAccessCalls.at(-1)).to.deep.equal(value);
		});
	});
});
