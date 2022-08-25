import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, map, switchMap, EMPTY, of } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import {
	createExtensionElement,
	UmbExtensionManifestEditor,
	UmbExtensionManifestTree,
	UmbExtensionRegistry,
} from '../../../core/extension';
import { UmbSectionContext } from '../section.context';

import '../shared/section-trees.element.ts';

@customElement('umb-section-members')
export class UmbSectionMembers extends UmbContextConsumerMixin(LitElement) {
	// TODO: make this code reusable across sections
	@state()
	private _routes: Array<any> = [];

	@state()
	private _trees?: Array<UmbExtensionManifestTree>;

	private _editors?: Array<UmbExtensionManifestEditor>;
	private _editorsSubscription?: Subscription;

	private _sectionContext?: UmbSectionContext;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionsRegistry: UmbExtensionRegistry) => {
			this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
				this._extensionRegistry = extensionsRegistry;
				this._sectionContext = sectionContext;
				this._useEditors();
				this._useTrees();
			});
		});
	}

	private _useEditors() {
		this._editorsSubscription?.unsubscribe();

		this._extensionRegistry?.extensionsOfType('editor').subscribe((editors) => {
			this._editors = editors;
		});
	}

	private _useTrees() {
		this._treesSubscription?.unsubscribe();

		this._treesSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionRegistry
							?.extensionsOfType('tree')
							.pipe(
								map((trees) =>
									trees
										.filter((tree) => tree.meta.sections.includes(section.alias))
										.sort((a, b) => b.meta.weight - a.meta.weight)
								)
							) ?? of([])
					);
				})
			)
			.subscribe((trees) => {
				this._trees = trees;
				this._createRoutes();
			});
	}

	private _createRoutes() {
		const treeRoutes =
			this._trees?.map((tree) => {
				// TODO: make this code respond to updates in editor extensions
				const editor = this._editors?.find((editor) => editor.alias === tree.meta.editor);
				// TODO: implement fallback editor
				const fallbackEditor = document.createElement('div');
				fallbackEditor.innerHTML = '<p>No editor found</p>';

				return {
					path: `${tree.meta.pathname}/:id`,
					component: () => (editor ? createExtensionElement(editor) : fallbackEditor),
				};
			}) ?? [];

		this._routes = [
			...treeRoutes,
			{
				path: 'dashboard',
				component: () => import('../shared/section-dashboards.element'),
			},
			{
				path: '**',
				redirectTo: 'dashboard',
			},
		];
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._editorsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-section-layout>
				<umb-section-sidebar>
					<umb-section-trees></umb-section-trees>
				</umb-section-sidebar>
				<umb-section-main>
					<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
				</umb-section-main>
			</umb-section-layout>
		`;
	}
}

export default UmbSectionMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-members': UmbSectionMembers;
	}
}
