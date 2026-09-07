import type { ShowFieldsModalData } from './types.js';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const MODAL_ALIAS = 'Umbraco.Cms.Search.Modal.DocumentFields';

/** Module-level export shared with the entity action for URL construction. */
export let fieldsRouteBuilder: UmbModalRouteBuilder | undefined;

@customElement('umb-examine-fields-route-provider')
export class UmbExamineFieldsRouteProviderElement extends UmbLitElement {
  #indexAlias?: string;

  constructor() {
    super();

    this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
      if (!context) return;

      // Avoid re-initializing if the context callback is invoked multiple times.
      if (this.#indexAlias) return;

      this.#indexAlias = context.getUnique() ?? undefined;

      new UmbModalRouteRegistrationController<ShowFieldsModalData>(this, MODAL_ALIAS)
        .addAdditionalPath(':documentUnique/:culture')
        .onSetup((params) => {
          return {
            modal: {
              type: 'sidebar',
              size: 'large',
            },
            data: {
              documentUnique: params.documentUnique,
              indexAlias: this.#indexAlias ?? '',
              culture: params.culture,
            },
            value: undefined,
          };
        })
        .observeRouteBuilder((routeBuilder) => {
          fieldsRouteBuilder = routeBuilder ?? undefined;
        });
    });
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    fieldsRouteBuilder = undefined;
  }

  override render() {
    return nothing;
  }
}

export default UmbExamineFieldsRouteProviderElement;
