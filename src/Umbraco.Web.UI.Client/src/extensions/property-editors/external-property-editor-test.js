export default class ExternalPropertyEditorTest extends HTMLElement {
  
    constructor() {
        super();
        this.attachShadow({mode: 'open'}); // sets and returns 'this.shadowRoot'
        const wrapper = document.createElement('span');
        wrapper.textContent = 'hello world';
        this.shadowRoot.append(wrapper);
    }
}
customElements.define('external-property-editor-test', ExternalPropertyEditorTest);