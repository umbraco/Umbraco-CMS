import { property } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import type { PropertyValuePresentationDisplayOption } from "src/packages/core/property-value-presentation/property-value-presentation.extension";

export abstract class UmbPropertyValuePresentationBase extends UmbLitElement{
  @property()
  alias: string = "";

  @property()
  display?: PropertyValuePresentationDisplayOption;

  @property()
  value: any;
}