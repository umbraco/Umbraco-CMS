import { property } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import type { UmbPropertyValuePresentationDisplayOption } from "src/packages/core/property-value-presentation/property-value-presentation.extension";

export abstract class UmbPropertyValuePresentationBaseElement extends UmbLitElement{
  @property()
  alias: string = "";

  @property()
  display?: UmbPropertyValuePresentationDisplayOption;

  @property()
  value: any;
}