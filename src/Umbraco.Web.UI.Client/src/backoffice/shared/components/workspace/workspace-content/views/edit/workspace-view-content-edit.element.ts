import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';
import { UmbWorkspaceContentContext } from '../../workspace-content.context';
import type { ContentProperty, ContentPropertyData, DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import '../../../../content-property/content-property.element';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-workspace-view-content-edit')
export class UmbWorkspaceViewContentEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	@state()
	_properties: ContentProperty[] = [];

	@state()
	_data: ContentPropertyData[] = [];

	private _workspaceContext?: UmbWorkspaceContentContext<DocumentDetails | MediaDetails>;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceContentContext<DocumentDetails | MediaDetails>>(
			'umbWorkspaceContext',
			(workspaceContext) => {
				this._workspaceContext = workspaceContext;
				this._observeContent();
			}
		);
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		/*
		TODO: Property-Context: This observer gets all changes, We need to fix this. it should be simpler.
		It should look at length and aliases? as long as they are identical nothing should change.
		As they would update them selfs?

		Should use a Observable for this._workspaceContext.properties
		*/
		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (content) => {
			this._properties = content.properties;
			this._data = content.data;
			/*
				Maybe we should not give the value, but the umb-content-property should get the context and observe its own data.
				This would become a more specific Observer therefor better performance?.. Note to self: Debate with Mads how he sees this perspective.
				*/
		});
	}

	render() {
		return html`
			<uui-box>
				${repeat(
					this._properties,
					(property) => property.alias,
					(property) =>
						html`<umb-content-property
							.property=${property}
							.value=${this._data.find((data) => data.alias === property.alias)?.value}></umb-content-property> `
				)}
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewContentEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-content-edit': UmbWorkspaceViewContentEditElement;
	}
}
