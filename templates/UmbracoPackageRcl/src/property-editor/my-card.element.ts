import {UUICardElement} from "@umbraco-cms/backoffice/external/uui";
import {css} from "lit";
import {customElement, property} from "lit/decorators.js";

@customElement("my-card")
export class MyCardElement extends UUICardElement {

    @property()
    value!: string;

    constructor() {
        super();
        this.selectable = true;
    }

    static styles = [
        ...UUICardElement.styles,
        css`
          :host {
            padding: 1rem;
            width: 200px;
          }
        `
    ]
}