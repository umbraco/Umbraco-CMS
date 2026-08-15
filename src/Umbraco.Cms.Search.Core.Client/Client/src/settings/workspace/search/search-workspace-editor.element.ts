import { UMB_SEARCH_ROOT_WORKSPACE_PATH } from '../paths.js';
import { UMB_SEARCH_WORKSPACE_CONTEXT } from './search-workspace.context-token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-search-workspace-editor')
export class UmbSearchWorkspaceEditorElement extends UmbLitElement {
  #workspaceContext?: typeof UMB_SEARCH_WORKSPACE_CONTEXT.TYPE;

  @state()
  private _indexAlias?: string;

  constructor() {
    super();

    this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (context) => {
      this.#workspaceContext = context;
      this.#observeIndexAlias();
    });
  }

  #observeIndexAlias() {
    this.observe(
      this.#workspaceContext?.name,
      (alias) => {
        this._indexAlias = alias;
      },
      '_observeIndexAlias',
    );
  }

  override render() {
    return html`
      <umb-entity-detail-workspace-editor .backPath=${UMB_SEARCH_ROOT_WORKSPACE_PATH}>
        <h3 slot="header">${this._indexAlias ?? 'Loading...'}</h3>
      </umb-entity-detail-workspace-editor>
    `;
  }

  static override readonly styles = [UmbTextStyles];
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-search-workspace-editor': UmbSearchWorkspaceEditorElement;
  }
}
