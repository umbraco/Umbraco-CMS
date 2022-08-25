import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, Subscription, first } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionManifestTree, UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';

import '../../tree/tree.element';

@customElement('umb-section-trees')
export class UmbSectionTrees extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	@state()
	private _trees: Array<UmbExtensionManifestTree> = [];

	private _currentSectionAlias = '';

	private _extensionStore?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	private _sectionContext?: UmbSectionContext;
	private _sectionContextSubscription?: Subscription;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
			this._extensionStore = _instance;
			this._useTrees();
		});

		this.consumeContext('umbSectionContext', (context: UmbSectionContext) => {
			this._sectionContext = context;
			this._useSectionContext();
		});
	}

	private _useSectionContext() {
		this._sectionContextSubscription?.unsubscribe();

		this._sectionContextSubscription = this._sectionContext?.data.pipe(first()).subscribe((section) => {
			this._currentSectionAlias = section.alias;
			this._useTrees();
		});
	}

	private _useTrees() {
		//TODO: Merge streams
		if (this._extensionStore && this._currentSectionAlias) {
			this._treesSubscription?.unsubscribe();

			this._treesSubscription = this._extensionStore
				?.extensionsOfType('tree')
				.pipe(
					map((extensions) =>
						extensions
							.filter((extension) => extension.meta.sections.includes(this._currentSectionAlias as string)) // TODO: Why do whe need "as string" here??
							.sort((a, b) => b.meta.weight - a.meta.weight)
					)
				)
				.subscribe((treeExtensions) => {
					this._trees = treeExtensions;
				});
		}
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._sectionContextSubscription?.unsubscribe();
	}

	render() {
		return html`${this._trees.map((tree) => html`<umb-tree .tree=${tree}></umb-tree>`)} `;
	}
}

export default UmbSectionTrees;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-trees': UmbSectionTrees;
	}
}
