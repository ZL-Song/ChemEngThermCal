namespace ChemEngThermCal.View {
    class CprDiagInterator : Model.CorStt.Models.Diagram.IDiagramInteractor {
        private Model.CorStt.Models.Diagram.InteractResult _interactResult;

        private bool _isAborted;

        public bool IsAborted
            => _isAborted;

        public Model.CorStt.Models.Diagram.InteractResult GetInteracResult()
            => _interactResult;

        public void Initialize(double relativeTemperature, double relativePressure) {
            PfGceCprDiagView view = new PfGceCprDiagView(relativeTemperature, relativePressure);
            view.ShowDialog();
            if (view.DialogResult == true) {
                _isAborted = false;
                _interactResult = new Model.CorStt.Models.Diagram.InteractResult(view.Base, view.Crec);
            } else {
                _isAborted = true;
                _interactResult = new Model.CorStt.Models.Diagram.InteractResult(-3.0, -3.0);
            }
        }
    }
}
