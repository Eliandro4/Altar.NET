namespace Altar
{
    public class ExportOptions
    {
        public bool General { get; set; }
        public bool Options { get; set; }
        public bool Sound { get; set; }
        public bool Sprite { get; set; }
        public bool Background { get; set; }
        public bool Path { get; set; }
        public bool Script { get; set; }
        public bool Font { get; set; }
        public bool Object { get; set; }
        public bool Room { get; set; }
        public bool TPag { get; set; }
        public bool Texture { get; set; }
        public bool Audio { get; set; }
        public bool Decompile { get; set; }
        public bool Disassemble { get; set; }
        public bool String { get; set; }
        public bool Variables { get; set; }
        public bool Functions { get; set; }
        public bool AudioGroups { get; set; }
        public bool Shader { get; set; }
        public bool Any { get; set; }
        public bool AbsoluteAddresses { get; set; }
        public bool ExportToProject { get; set; }
        public string File { get; set; } = "data.win";
        public string OutputDirectory { get; set; }
        public bool DumpUnknownChunks { get; set; }
        public bool DumpEmptyChunks { get; set; }
        public bool DumpAllChunks { get; set; }
        public bool Quiet { get; set; }
        public bool NoPrecProg { get; set; }
        public bool DetachedAgrp { get; set; }
        public bool DumpTPagPNGs { get; set; }
        public bool DumpSpritePNGs { get; set; }
    }

    class ImportOptions
    {
        public string File { get; set; }
        public string OutputFile { get; set; }
    }
}
