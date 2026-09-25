import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../../entity.js';
import { UmbUserGroupWorkspaceAssignAccessElement } from './user-group-workspace-assign-access.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type { UmbStartNodeAccessValue } from '@umbraco-cms/backoffice/property-editor';

/** Stands in for `UmbUserGroupWorkspaceContext`, exposing only the access-related state the element reads and writes. */
class UmbTestUserGroupWorkspaceContext {
	#host: UmbControllerHostElement;

	#sections = new UmbArrayState<string>([], (x) => x);
	readonly sections = this.#sections.asObservable();

	#hasAccessToAllLanguages = new UmbBooleanState(false);
	readonly hasAccessToAllLanguages = this.#hasAccessToAllLanguages.asObservable();
	#languages = new UmbArrayState<string>([], (x) => x);
	readonly languages = this.#languages.asObservable();

	#documentRootAccess = new UmbBooleanState(false);
	readonly documentRootAccess = this.#documentRootAccess.asObservable();
	#documentStartNode = new UmbObjectState<{ unique: string } | null>(null);
	readonly documentStartNode = this.#documentStartNode.asObservable();

	#mediaRootAccess = new UmbBooleanState(false);
	readonly mediaRootAccess = this.#mediaRootAccess.asObservable();
	#mediaStartNode = new UmbObjectState<{ unique: string } | null>(null);
	readonly mediaStartNode = this.#mediaStartNode.asObservable();

	readonly setSectionsCalls: Array<Array<string>> = [];
	readonly setLanguageAccessCalls: Array<UmbStartNodeAccessValue> = [];
	readonly setDocumentAccessCalls: Array<UmbStartNodeAccessValue> = [];
	readonly setMediaAccessCalls: Array<UmbStartNodeAccessValue> = [];

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	// The context token only resolves providers whose entity type matches the user group workspace.
	getEntityType() {
		return UMB_USER_GROUP_ENTITY_TYPE;
	}

	setSectionsState(sections: Array<string>) {
		this.#sections.setValue(sections);
	}

	setLanguageAccessState(value: UmbStartNodeAccessValue) {
		this.#hasAccessToAllLanguages.setValue(value.rootAccess);
		this.#languages.setValue(value.startNodes.map((startNode) => startNode.unique));
	}

	setDocumentAccessState(value: UmbStartNodeAccessValue) {
		this.#documentRootAccess.setValue(value.rootAccess);
		this.#documentStartNode.setValue(value.startNodes[0] ? { unique: value.startNodes[0].unique } : null);
	}

	setMediaAccessState(value: UmbStartNodeAccessValue) {
		this.#mediaRootAccess.setValue(value.rootAccess);
		this.#mediaStartNode.setValue(value.startNodes[0] ? { unique: value.startNodes[0].unique } : null);
	}

	setSections(sections: Array<string>) {
		this.setSectionsCalls.push(sections);
	}

	setLanguageAccess(value: UmbStartNodeAccessValue) {
		this.setLanguageAccessCalls.push(value);
	}

	setDocumentAccess(value: UmbStartNodeAccessValue) {
		this.setDocumentAccessCalls.push(value);
	}

	setMediaAccess(value: UmbStartNodeAccessValue) {
		this.setMediaAccessCalls.push(value);
	}
}

@customElement('umb-test-user-group-assign-access-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestAssignAccessHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbUserGroupWorkspaceAssignAccessElement', () => {
	let host: UmbTestAssignAccessHostElement;
	let element: UmbUserGroupWorkspaceAssignAccessElement;
	let context: UmbTestUserGroupWorkspaceContext;
	let dataset: UmbPropertyDatasetContext;

	beforeEach(async () => {
		host = await fixture(
			html`<umb-test-user-group-assign-access-host>
				<umb-user-group-workspace-assign-access></umb-user-group-workspace-assign-access>
			</umb-test-user-group-assign-access-host>`,
		);
		element = host.querySelector(
			'umb-user-group-workspace-assign-access',
		) as UmbUserGroupWorkspaceAssignAccessElement;

		context = new UmbTestUserGroupWorkspaceContext(host);
		host.provideContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, context as never);
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
		expect(element).to.be.instanceOf(UmbUserGroupWorkspaceAssignAccessElement);
	});

	describe('workspace context → property dataset', () => {
		it('mirrors the selected sections', async () => {
			context.setSectionsState(['content', 'media']);
			await aTimeout(0);

			expect(await datasetValueByAlias('sections')).to.deep.equal(['content', 'media']);
		});

		it('merges language root access and the language selection into a single value', async () => {
			context.setLanguageAccessState({ rootAccess: false, startNodes: [{ unique: 'en-us' }] });
			await aTimeout(0);

			expect(await datasetValueByAlias('languageAccess')).to.deep.equal({
				rootAccess: false,
				startNodes: [{ unique: 'en-us' }],
			});
		});

		it('merges document root access and the single start node into a single value', async () => {
			context.setDocumentAccessState({ rootAccess: false, startNodes: [{ unique: 'doc-1' }] });
			await aTimeout(0);

			expect(await datasetValueByAlias('documentAccess')).to.deep.equal({
				rootAccess: false,
				startNodes: [{ unique: 'doc-1' }],
			});
		});

		it('merges media root access and the single start node into a single value', async () => {
			context.setMediaAccessState({ rootAccess: true, startNodes: [] });
			await aTimeout(0);

			expect(await datasetValueByAlias('mediaAccess')).to.deep.equal({ rootAccess: true, startNodes: [] });
		});

		it('does not write workspace-originated values back to the workspace context', async () => {
			// A value pushed in from the workspace (e.g. the workspace data being cleared after deletion) must not
			// echo back into a `set*` call, or an unrelated navigation will look like it has unpersisted changes.
			context.setSectionsState(['content']);
			context.setLanguageAccessState({ rootAccess: false, startNodes: [{ unique: 'en-us' }] });
			context.setDocumentAccessState({ rootAccess: false, startNodes: [{ unique: 'doc-1' }] });
			context.setMediaAccessState({ rootAccess: true, startNodes: [] });
			await aTimeout(0);

			expect(context.setSectionsCalls).to.be.empty;
			expect(context.setLanguageAccessCalls).to.be.empty;
			expect(context.setDocumentAccessCalls).to.be.empty;
			expect(context.setMediaAccessCalls).to.be.empty;
		});
	});

	describe('property dataset → workspace context', () => {
		it('writes a changed sections selection back to the workspace context', async () => {
			dataset.setPropertyValue('sections', ['content']);
			await aTimeout(0);

			expect(context.setSectionsCalls.at(-1)).to.deep.equal(['content']);
		});

		it('writes a changed language access value back to the workspace context', async () => {
			const value: UmbStartNodeAccessValue = { rootAccess: true, startNodes: [] };
			dataset.setPropertyValue('languageAccess', value);
			await aTimeout(0);

			expect(context.setLanguageAccessCalls.at(-1)).to.deep.equal(value);
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
	});
});
